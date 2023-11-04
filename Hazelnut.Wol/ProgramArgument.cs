using System.Net;

namespace Hazelnut.Wol;

public class ProgramArgument
{
    public bool Help = false;
    
    public readonly IPAddress BroadcastAddress = IPAddress.Broadcast;
    public readonly ushort Port = 9;
    public readonly string Password = string.Empty;
    public readonly bool Quiet = false;
    public readonly string[] HardwareAddresses = Array.Empty<string>();

    public ProgramArgument()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--help") || args.Contains("-h"))
        {
            Help = true;
            return;
        }

        var broadcastOption = args.LastOrDefault(arg => arg.StartsWith("--broadcast=") || arg.StartsWith("-b="));
        if (broadcastOption != null)
            Help = !IPAddress.TryParse(broadcastOption.AsSpan(broadcastOption.IndexOf('=') + 1), out BroadcastAddress);
        if (Help)
            return;

        var portOption = args.LastOrDefault(arg => arg.StartsWith("--port=") || arg.StartsWith("-p="));
        if (portOption != null)
            Help = !ushort.TryParse(portOption.AsSpan(portOption.IndexOf('=') + 1), out Port);
        if (Help)
            return;

        var passwordOption = args.LastOrDefault(arg => arg.StartsWith("--password=") || arg.StartsWith("-w="));
        if (passwordOption != null)
        {
            Password = passwordOption[(passwordOption.IndexOf('=') + 1)..];
            if (Password.Length > 6)
                Help = true;
            if (!Password.All(char.IsAscii))
                Help = true;
        }
        if (Help)
            return;

        var quietOption = args.Contains("--quiet") || args.Contains("-q");
        Quiet = quietOption;

        HardwareAddresses = args.Where(WakeOnLan.IsHardwareAddressFormat).ToArray();
        Help = HardwareAddresses.Length == 0;
    }
}