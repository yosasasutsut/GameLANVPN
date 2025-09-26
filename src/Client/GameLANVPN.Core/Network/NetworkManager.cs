using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace GameLANVPN.Core.Network;

public class NetworkManager : INetworkManager, IDisposable
{
    private readonly ILogger<NetworkManager> _logger;
    private readonly VirtualAdapter _virtualAdapter;
    private HubConnection? _hubConnection;
    private readonly ConcurrentDictionary<string, IPEndPoint> _playerEndpoints = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event EventHandler<PacketReceivedEventArgs>? PacketReceived;
    public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public string? CurrentRoomCode { get; private set; }

    private string? _virtualIP;
    private string? _playerName;

    public NetworkManager(ILogger<NetworkManager> logger)
    {
        _logger = logger;
        _virtualAdapter = new VirtualAdapter();
        _virtualAdapter.PacketCaptured += OnPacketCaptured;
    }

    public async Task<bool> ConnectToServerAsync(IPEndPoint serverEndpoint, string roomCode)
    {
        try
        {
            var serverUrl = $"http://{serverEndpoint.Address}:{serverEndpoint.Port}/gamehub";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect()
                .Build();

            SetupHubHandlers();

            await _hubConnection.StartAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("Connected to server at {ServerUrl}", serverUrl);

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs
            {
                IsConnected = true,
                Message = "Connected to server"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to server");

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs
            {
                IsConnected = false,
                Message = $"Connection failed: {ex.Message}"
            });

            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(CurrentRoomCode))
            {
                await _hubConnection?.InvokeAsync("LeaveRoom", CurrentRoomCode)!;
                CurrentRoomCode = null;
            }

            _virtualAdapter.StopCapture();

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs
            {
                IsConnected = false,
                Message = "Disconnected from server"
            });

            _logger.LogInformation("Disconnected from server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnection");
        }
    }

    public async Task<bool> SendPacketAsync(byte[] data, IPEndPoint destination)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return false;

        try
        {
            var targetIP = destination.Address.ToString();

            if (targetIP == "255.255.255.255" || targetIP.StartsWith("224."))
            {
                await _hubConnection.InvokeAsync("BroadcastPacket", CurrentRoomCode, data);
            }
            else
            {
                await _hubConnection.InvokeAsync("RelayPacket", CurrentRoomCode, data, targetIP);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send packet");
            return false;
        }
    }

    public async Task<RoomInfo?> CreateRoomAsync(string gameName, int maxPlayers)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return null;

        try
        {
            _playerName = Environment.UserName;
            var result = await _hubConnection.InvokeAsync<dynamic>("CreateRoom", gameName, _playerName, maxPlayers);

            if (result.Success)
            {
                CurrentRoomCode = result.RoomCode;
                _virtualIP = result.VirtualIP;

                await InitializeVirtualAdapter();

                return new RoomInfo
                {
                    RoomCode = result.RoomCode,
                    GameName = gameName,
                    HostName = _playerName,
                    MaxPlayers = maxPlayers,
                    CurrentPlayers = 1,
                    CreatedAt = DateTime.Now
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create room");
        }

        return null;
    }

    public async Task<bool> JoinRoomAsync(string roomCode)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return false;

        try
        {
            _playerName = Environment.UserName;
            var result = await _hubConnection.InvokeAsync<dynamic>("JoinRoom", roomCode, _playerName);

            if (result.Success)
            {
                CurrentRoomCode = roomCode;
                _virtualIP = result.VirtualIP;

                await InitializeVirtualAdapter();

                _logger.LogInformation("Joined room {RoomCode} with IP {VirtualIP}", roomCode, _virtualIP);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to join room: {Error}", (string)result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join room");
        }

        return false;
    }

    public async Task<List<RoomInfo>> GetAvailableRoomsAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return new List<RoomInfo>();

        try
        {
            var rooms = await _hubConnection.InvokeAsync<List<dynamic>>("GetAvailableRooms");
            return rooms.Select(r => new RoomInfo
            {
                RoomCode = r.RoomCode,
                GameName = r.GameName,
                HostName = r.HostName,
                CurrentPlayers = r.CurrentPlayers,
                MaxPlayers = r.MaxPlayers,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available rooms");
            return new List<RoomInfo>();
        }
    }

    private void SetupHubHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<byte[], string>("ReceivePacket", async (packetData, sourceVirtualIP) =>
        {
            try
            {
                await InjectPacketToVirtualAdapter(packetData, sourceVirtualIP);

                PacketReceived?.Invoke(this, new PacketReceivedEventArgs
                {
                    Data = packetData,
                    Source = new IPEndPoint(IPAddress.Parse(sourceVirtualIP), 0),
                    ReceivedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing received packet");
            }
        });

        _hubConnection.On<dynamic>("PlayerJoined", (player) =>
        {
            _logger.LogInformation("Player joined: {PlayerName} ({VirtualIP})", (string)player.PlayerName, (string)player.VirtualIP);
        });

        _hubConnection.On<string>("PlayerLeft", (connectionId) =>
        {
            _logger.LogInformation("Player left: {ConnectionId}", connectionId);
        });

        _hubConnection.Reconnected += async (connectionId) =>
        {
            _logger.LogInformation("Reconnected to server");
            if (!string.IsNullOrEmpty(CurrentRoomCode))
            {
                await JoinRoomAsync(CurrentRoomCode);
            }
        };
    }

    private async Task InitializeVirtualAdapter()
    {
        if (!await _virtualAdapter.InitializeAsync())
        {
            _logger.LogWarning("Failed to initialize virtual adapter, using fallback mode");
            return;
        }

        _virtualAdapter.StartCapture();
        _logger.LogInformation("Virtual adapter initialized and capturing started");
    }

    private void OnPacketCaptured(object? sender, PacketEventArgs e)
    {
        try
        {
            if (e.ParsedPacket?.PayloadPacket != null)
            {
                var destIP = ExtractDestinationIP(e.PacketData);
                if (destIP != null)
                {
                    Task.Run(async () => await SendPacketAsync(e.PacketData, destIP));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing captured packet");
        }
    }

    private async Task InjectPacketToVirtualAdapter(byte[] packetData, string sourceVirtualIP)
    {
        try
        {
            var modifiedPacket = ModifyPacketSource(packetData, sourceVirtualIP);
            _virtualAdapter.SendPacket(modifiedPacket);

            _logger.LogDebug("Injected packet from {SourceIP}, size: {Size}", sourceVirtualIP, packetData.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject packet to virtual adapter");
        }
    }

    private IPEndPoint? ExtractDestinationIP(byte[] packetData)
    {
        try
        {
            if (packetData.Length < 20) return null;

            var destBytes = new byte[4];
            Array.Copy(packetData, 16, destBytes, 0, 4);
            var destIP = new IPAddress(destBytes);

            var destPort = BitConverter.ToUInt16(packetData, 22);
            if (BitConverter.IsLittleEndian)
                destPort = (ushort)IPAddress.NetworkToHostOrder((short)destPort);

            return new IPEndPoint(destIP, destPort);
        }
        catch
        {
            return null;
        }
    }

    private byte[] ModifyPacketSource(byte[] originalPacket, string sourceVirtualIP)
    {
        try
        {
            var modifiedPacket = new byte[originalPacket.Length];
            Array.Copy(originalPacket, modifiedPacket, originalPacket.Length);

            var sourceIPBytes = IPAddress.Parse(sourceVirtualIP).GetAddressBytes();
            Array.Copy(sourceIPBytes, 0, modifiedPacket, 12, 4);

            return modifiedPacket;
        }
        catch
        {
            return originalPacket;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        DisconnectAsync().Wait(5000);
        _virtualAdapter?.Dispose();
        _cancellationTokenSource.Dispose();
    }
}