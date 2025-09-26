using System.Net;
using System.Net.NetworkInformation;
using SharpPcap;
using PacketDotNet;

namespace GameLANVPN.Core.Network;

public class VirtualAdapter
{
    private ICaptureDevice? _device;
    private readonly string _adapterName;
    private bool _isCapturing;

    public event EventHandler<PacketEventArgs>? PacketCaptured;

    public VirtualAdapter(string adapterName = "GameLANVPN Virtual Adapter")
    {
        _adapterName = adapterName;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var devices = CaptureDeviceList.Instance;

            _device = devices.FirstOrDefault(d =>
                d.Description?.Contains("TAP-Windows Adapter", StringComparison.OrdinalIgnoreCase) == true ||
                d.Description?.Contains(_adapterName, StringComparison.OrdinalIgnoreCase) == true);

            if (_device == null)
            {
                return false;
            }

            _device.OnPacketArrival += OnPacketArrival;
            _device.Open(DeviceModes.Promiscuous, 1000);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void StartCapture()
    {
        if (_device != null && !_isCapturing)
        {
            _device.StartCapture();
            _isCapturing = true;
        }
    }

    public void StopCapture()
    {
        if (_device != null && _isCapturing)
        {
            _device.StopCapture();
            _isCapturing = false;
        }
    }

    public void SendPacket(byte[] packetData)
    {
        try
        {
            if (_device is IInjectionDevice injectionDevice)
            {
                injectionDevice.SendPacket(packetData);
            }
        }
        catch
        {
            // Fallback: ignore if injection not supported
        }
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        var packet = e.GetPacket();
        var parsedPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);

        PacketCaptured?.Invoke(this, new PacketEventArgs
        {
            PacketData = packet.Data,
            ParsedPacket = parsedPacket,
            Timestamp = packet.Timeval.Date
        });
    }

    public void Dispose()
    {
        StopCapture();
        _device?.Close();
        _device?.Dispose();
    }
}

public class PacketEventArgs : EventArgs
{
    public byte[] PacketData { get; set; } = Array.Empty<byte>();
    public Packet? ParsedPacket { get; set; }
    public DateTime Timestamp { get; set; }
}