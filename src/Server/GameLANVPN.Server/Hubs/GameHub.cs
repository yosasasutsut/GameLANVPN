using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace GameLANVPN.Server.Hubs;

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, Room> _rooms = new();
    private static readonly ConcurrentDictionary<string, Player> _players = new();

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_players.TryRemove(Context.ConnectionId, out var player))
        {
            if (!string.IsNullOrEmpty(player.RoomCode))
            {
                await LeaveRoom(player.RoomCode);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<RoomCreatedResult> CreateRoom(string gameName, string playerName, int maxPlayers)
    {
        var roomCode = GenerateRoomCode();
        var room = new Room
        {
            RoomCode = roomCode,
            GameName = gameName,
            MaxPlayers = maxPlayers,
            HostConnectionId = Context.ConnectionId,
            CreatedAt = DateTime.UtcNow
        };

        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            PlayerName = playerName,
            RoomCode = roomCode,
            IsHost = true,
            VirtualIP = GenerateVirtualIP(roomCode, 1)
        };

        room.Players.Add(player);
        _rooms.TryAdd(roomCode, room);
        _players.TryAdd(Context.ConnectionId, player);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.Group(roomCode).SendAsync("PlayerJoined", player);

        return new RoomCreatedResult
        {
            Success = true,
            RoomCode = roomCode,
            VirtualIP = player.VirtualIP
        };
    }

    public async Task<JoinResult> JoinRoom(string roomCode, string playerName)
    {
        if (!_rooms.TryGetValue(roomCode, out var room))
        {
            return new JoinResult { Success = false, Error = "Room not found" };
        }

        if (room.Players.Count >= room.MaxPlayers)
        {
            return new JoinResult { Success = false, Error = "Room is full" };
        }

        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            PlayerName = playerName,
            RoomCode = roomCode,
            IsHost = false,
            VirtualIP = GenerateVirtualIP(roomCode, room.Players.Count + 1)
        };

        room.Players.Add(player);
        _players.TryAdd(Context.ConnectionId, player);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.Group(roomCode).SendAsync("PlayerJoined", player);
        await Clients.Caller.SendAsync("RoomJoined", room);

        return new JoinResult
        {
            Success = true,
            VirtualIP = player.VirtualIP,
            Players = room.Players.ToList()
        };
    }

    public async Task LeaveRoom(string roomCode)
    {
        if (_players.TryRemove(Context.ConnectionId, out var player))
        {
            if (_rooms.TryGetValue(roomCode, out var room))
            {
                room.Players.RemoveAll(p => p.ConnectionId == Context.ConnectionId);

                if (room.Players.Count == 0)
                {
                    _rooms.TryRemove(roomCode, out _);
                }
                else if (player.IsHost)
                {
                    var newHost = room.Players.First();
                    newHost.IsHost = true;
                    room.HostConnectionId = newHost.ConnectionId;
                    await Clients.Group(roomCode).SendAsync("HostChanged", newHost);
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync("PlayerLeft", Context.ConnectionId);
            }
        }
    }

    public async Task RelayPacket(string roomCode, byte[] packetData, string targetVirtualIP)
    {
        if (_rooms.TryGetValue(roomCode, out var room))
        {
            var targetPlayer = room.Players.FirstOrDefault(p => p.VirtualIP == targetVirtualIP);
            if (targetPlayer != null)
            {
                await Clients.Client(targetPlayer.ConnectionId).SendAsync("ReceivePacket", packetData);
            }
        }
    }

    public async Task BroadcastPacket(string roomCode, byte[] packetData)
    {
        if (_players.TryGetValue(Context.ConnectionId, out var sender))
        {
            await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("ReceivePacket", packetData, sender.VirtualIP);
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return _rooms.ContainsKey(code) ? GenerateRoomCode() : code;
    }

    private string GenerateVirtualIP(string roomCode, int playerIndex)
    {
        return $"10.{roomCode.GetHashCode() % 256}.{playerIndex / 256}.{playerIndex % 256}";
    }
}

public class Room
{
    public string RoomCode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string HostConnectionId { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public List<Player> Players { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class Player
{
    public string ConnectionId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string VirtualIP { get; set; } = string.Empty;
    public bool IsHost { get; set; }
}

public class RoomCreatedResult
{
    public bool Success { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string VirtualIP { get; set; } = string.Empty;
}

public class JoinResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string VirtualIP { get; set; } = string.Empty;
    public List<Player> Players { get; set; } = new();
}