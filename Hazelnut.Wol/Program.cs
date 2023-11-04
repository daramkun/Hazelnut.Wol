using Hazelnut.Wol;

var arguments = new ProgramArgument();
if (arguments.Help)
{
    Console.Error.WriteLine("USAGE:\n  {0} [-q] [-b=<broadcast address>] [-p=<port>] [-w=<password>] <destinations>",
        AppDomain.CurrentDomain.FriendlyName);
    return 1;
}

if (!arguments.Quiet)
{
    Console.Out.WriteLine(@"    __  __                 __            __ _       __      __
   / / / /___ _____  ___  / /___  __  __/ /| |     / /___  / /
  / /_/ / __ `/_  / / _ \/ / __ \/ / / / __/ | /| / / __ \/ / 
 / __  / /_/ / / /_/  __/ / / / / /_/ / /__| |/ |/ / /_/ / /  
/_/ /_/\__,_/ /___/\___/_/_/ /_/\__,_/\__(_)__/|__/\____/_/   ");
    if (string.IsNullOrEmpty(arguments.Password))
        Console.Out.WriteLine("- WOL will send to {0}:{1}", arguments.BroadcastAddress, arguments.Port);
    else
        Console.Out.WriteLine("- WOL will send to {0}:{1} with password {2}", arguments.BroadcastAddress, arguments.Port, arguments.Password);
}

using var wol = new WakeOnLan(arguments.BroadcastAddress, arguments.Port);
foreach (var hardwareAddress in arguments.HardwareAddresses)
{
    wol.SendWakeOnLanPacket(hardwareAddress, arguments.Password);
    if (!arguments.Quiet)
        Console.Out.WriteLine("- Packet sent to {0}", hardwareAddress);
}

if (!arguments.Quiet)
    Console.Out.WriteLine("- WOL Packet sent completely.");

return 0;