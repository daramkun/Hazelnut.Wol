using System.Collections.Immutable;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Hazelnut.Wol;

public class WakeOnLan : IDisposable
{
    private static readonly Regex _hardwareAddressFormat1 = new Regex("^[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]$");
    private static readonly Regex _hardwareAddressFormat2 = new Regex("^[0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f]$");

    public static bool IsHardwareAddressFormat(string text)
    {
        return _hardwareAddressFormat1.IsMatch(text) ||
               _hardwareAddressFormat2.IsMatch(text);
    }
    
    private readonly Socket _socket;
    private readonly EndPoint _wolEndPoint;

    public WakeOnLan()
        : this(IPAddress.Broadcast, 9)
    {

    }

    public WakeOnLan(IPAddress broadcastAddress, ushort port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        _wolEndPoint = new IPEndPoint(broadcastAddress, port);
    }

    ~WakeOnLan()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _socket.Dispose();
    }
    
    public void SendWakeOnLanPacket(string hardwareAddress, string password)
    {
        if (!_hardwareAddressFormat1.IsMatch(hardwareAddress) &&
            !_hardwareAddressFormat2.IsMatch(hardwareAddress))
            throw new FormatException("Hardware Address format is not matched.");
        
        Span<byte> packetMessage = stackalloc byte[6 * 17 + ((password.Length > 0) ? 6 : 0)];
        packetMessage.Slice(0, 6).Fill(0xFF);
        if (password.Length > 0)
        {
            var passwordArea = packetMessage.Slice(6 * 17, 6);
            for (var i = 0; i < password.Length; ++i)
                passwordArea[i] = (byte)password[i];
        }
        
        HardwareAddressToBytes(hardwareAddress, packetMessage.Slice(6, 6));

        for (var i = 1; i <= 16; ++i)
            packetMessage.Slice(6, 6).CopyTo(packetMessage.Slice(i * 6, 6));

        _socket.SendTo(packetMessage, _wolEndPoint);
    }

    private void HardwareAddressToBytes(string hardwareAddress, Span<byte> buffer)
    {
        var index = 0;

        for (var i = 0; i < hardwareAddress.Length;)
        {
            var ch1 = hardwareAddress[i];
            var ch2 = hardwareAddress[i + 1];

            var hex = (byte)(((CharToInt(ch1) << 4) | CharToInt(ch2)) & 0xFF);
            buffer[index++] = hex;
            
            i += 3;
        }
    }

    private static uint CharToInt(char ch)
    {
        if (char.IsUpper(ch))
            return (uint)(ch - 'A') + 10;
        if (char.IsLower(ch))
            return (uint)(ch - 'a') + 10;
        if (char.IsDigit(ch))
            return (uint)(ch - '0');
        throw new ArgumentOutOfRangeException(nameof(ch));
    }
}