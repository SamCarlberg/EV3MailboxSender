using System;
using System.Threading;
using System.Management;
using System.Collections.Generic;
using System.IO;
using MonoBrick.EV3;
using MonoBrick;

public static class Program
{
    static Brick<Sensor, Sensor, Sensor, Sensor> ev3 = null;

    static void Main(string[] args)
    {
        Console.OpenStandardOutput();
        Console.OpenStandardInput();
        Console.WriteLine("Starting C# backend");
        String input = "";
        connect();
        while (true)
        {
            input = Console.In.ReadLine();
            if (input.Equals("exit") || input.Equals("q"))
            {
                Console.WriteLine("Exiting C# backend.");
                break;
            }
            else if (input.Equals("connect"))
            {
                connect();
                continue;
            }
            else
            {
                String[] cmd = input.Split(':');
                if (cmd.Length > 2)
                {
                    Console.WriteLine("Invalid arg: " + input);
                    continue;
                }

                try
                {
                    if (cmd.Length == 2)
                    {
                        sendCommand(int.Parse(cmd[0]));
                        sendOperand(double.Parse(cmd[1]));
                    }
                    else
                    {
                        sendCommand(int.Parse(input));
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid arg");
                }
            }
        }
    }

    static void sendCommand(int command)
    {
        Console.WriteLine("Sending command #" + command);
        if (ev3 == null)
            return;
        ev3.Mailbox.Send("command", command + "");
    }

    static void sendOperand(double operand)
    {
        Console.WriteLine("\tOperand " + operand);
        if (ev3 == null)
            return;
        ev3.Mailbox.Send("operand", operand + "");
    }
    // Make sure you have a reference to System.Management or this won't compile
    //
    // Query code from https://32feet.codeplex.com/
    // Modified by Xander Soldaat
    static List<String> scan()
    {
        // The WMI query that makes it all happen
        const string QueryString = "SELECT Caption,PNPDeviceID FROM Win32_PnPEntity " +
            "WHERE ConfigManagerErrorCode = 0 AND " +
            "Caption LIKE 'Standard Serial over Bluetooth link (COM%' AND " +
            "PNPDeviceID LIKE '%&001653%'";

        SelectQuery WMIquery = new SelectQuery(QueryString);
        ManagementObjectSearcher WMIqueryResults = new ManagementObjectSearcher(WMIquery);
        if (WMIqueryResults != null)
        {
            List<String> comPorts = new List<String>();
            ManagementObjectCollection results = WMIqueryResults.Get();
            if (results.Count == 0)
            {
                Console.WriteLine("No NXTs found");
                return comPorts;
            }
            Console.WriteLine("The following NXTs were found on your system:");
            foreach (ManagementObject mo in results)
            {
                object captionObject = mo.GetPropertyValue("Caption");
                object pnpIdObject = mo.GetPropertyValue("PNPDeviceID");

                // Get the COM port name out of the Caption, requires a little search and replacing.
                string caption = captionObject.ToString();
                string comPort = caption.Substring(caption.LastIndexOf("(COM")).
                                    Replace("(", string.Empty).Replace(")", string.Empty);

                // Extract the BT address from the PNPObjectID property
                string BTaddress = pnpIdObject.ToString().Split('&')[4].Substring(0, 12);
                comPorts.Add(comPort);
            }
            return comPorts;
        }
        else
        {
            Console.WriteLine("Error executing query");
            return null;
        }
    }

    static void connect()
    {
        foreach (String port in scan())
        {
            try
            {
                ev3 = new Brick<Sensor, Sensor, Sensor, Sensor>(port);
                Console.WriteLine("\tTrying to connect to NXT on " + port);
                ev3.Connection.Open();
            }
            catch (ConnectionException e)
            {
                Console.WriteLine("\tCould not connect on " + port + ": " + e.Message);
                continue;
            }
            Console.WriteLine("connected");
            return;
        }
        Console.WriteLine("could not connect");
    }
}
