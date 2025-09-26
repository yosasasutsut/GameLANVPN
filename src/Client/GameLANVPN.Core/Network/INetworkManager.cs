using System.Net;

namespace GameLANVPN.Core.Network;

public interface INetworkManager
{
    event EventHandler<PacketReceivedEventArgs>? PacketReceived;
    event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    Task<bool> ConnectToServerAsync(IPEndPoint serverEndpoint, string roomCode);
    Task DisconnectAsync();
    Task<bool> SendPacketAsync(byte[] data, IPEndPoint destination);
    Task<RoomInfo?> CreateRoomAsync(string gameName, int maxPlayers);
    Task<bool> JoinRoomAsync(string roomCode);
    Task<List<RoomInfo>> GetAvailableRoomsAsync();
    bool IsConnected { get; }
    string? CurrentRoomCode { get; }
}

public class PacketReceivedEventArgs : EventArgs
{
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public IPEndPoint Source { get; init; } = null!;
    public DateTime ReceivedAt { get; init; }
}

public class ConnectionStateEventArgs : EventArgs
{
    public bool IsConnected { get; init; }
    public string? Message { get; init; }
}

public class RoomInfo
{
    public string RoomCode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PlayerInfo> Players { get; set; } = new();
}

public class PlayerInfo
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public IPEndPoint VirtualEndpoint { get; set; } = null!;
    public bool IsHost { get; set; }
    public DateTime JoinedAt { get; set; }
}