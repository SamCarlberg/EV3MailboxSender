using System;
using System.Threading;
using System.Management;
using System.Collections.Generic;
using System.IO;
using MonoBrick.EV3;
using MonoBrick;
using EV3MessengerLib;

public static class Program
{
    //static Brick<Sensor, Sensor, Sensor, Sensor> ev3 = null;
    private static EV3Messenger messenger = new EV3Messenger();

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
                    SendMessage("command", int.Parse(cmd[0]));
                    if (cmd.Length == 2)
                    {
                        SendMessage("operand", float.Parse(cmd[1]));
                    }
                    else
                    {
                        SendMessage("operand", 0);
                    }
                    while (messenger.ReadMessage() == null)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                    Console.WriteLine("Got response");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid arg");
                }
            }
        }
        messenger.Disconnect();
    }

    private static void SendMessage(String mailbox, String message)
    {
        Console.WriteLine("Sending message " + message + " to mailbox " + mailbox);
        if (messenger.IsConnected)
        {
            messenger.SendMessage(mailbox, message);
        }
    }

    private static void SendMessage(String mailbox, int message)
    {
        Console.WriteLine("Sending message " + message + " to mailbox " + mailbox);
        if (messenger.IsConnected)
        {
            messenger.SendMessage(mailbox, message);
        }
    }
    private static void SendMessage(String mailbox, float message)
    {
        Console.WriteLine("Sending message " + message + " to mailbox " + mailbox);
        if (messenger.IsConnected)
        {
            messenger.SendMessage(mailbox, message);
        }
    }

    private static void SendMessage(String mailbox, bool message)
    {
        Console.WriteLine("Sending message " + message + " to mailbox " + mailbox);
        if (messenger.IsConnected)
        {
            messenger.SendMessage(mailbox, message);
        }
    }

    // Make sure you have a reference to System.Management or this won't compile
    //
    // Query code from https://32feet.codeplex.com/
    // Modified by Xander Soldaat
    private static List<String> scan()
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
                Console.WriteLine("\t" + comPort);
            }
            return comPorts;
        }
        else
        {
            Console.WriteLine("Error executing query");
            return null;
        }
    }

    private static void connect()
    {
        foreach (String port in scan())
        {
            //ev3 = new Brick<Sensor, Sensor, Sensor, Sensor>(port);
            Console.WriteLine("\tTrying to connect to NXT on " + port);
            //ev3.Connection.Open();
            try
            {
                if (messenger.Connect(port))
                {
                    Console.WriteLine("connected");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                break;
            }
        }
        Console.WriteLine("could not connect");
    }
}
