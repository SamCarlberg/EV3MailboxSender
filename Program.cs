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

	static void Main (string[] args)
	{
		Console.OpenStandardOutput ();
		Console.OpenStandardInput ();
		Console.WriteLine ("Type 'q' or 'exit' to exit this C# program");
		String input;
		//connect();
		while (true) {
			input = Console.In.ReadLine ();
			//Console.WriteLine ("Input: " + input);
			if (input.Equals ("exit") || input.Equals ("q"))
				break;

			foreach (String str in input.Split(' ')) {
				String[] cmd = str.Split (':');
				if (cmd.Length > 2) {
					Console.WriteLine ("Invalid arg: " + str);
					continue;
				}

				if (cmd.Length == 2) {
					sendCommand (int.Parse (cmd [0]));
					sendOperand (double.Parse (cmd [1]));
				} else {
					sendCommand (int.Parse (str));
				}
			}
		}
		Console.WriteLine ("Exiting C# backend.");
	}

	static void sendCommand (int command)
	{
		Console.WriteLine ("Sending command #" + command);
		if (ev3 == null)
			return;
		ev3.Mailbox.Send ("mailbox1", command + "");
	}

	static void sendOperand (double operand)
	{
		Console.WriteLine ("\tOperand " + operand);
		if (ev3 == null)
			return;
		ev3.Mailbox.Send ("mailbox1", operand + "");
	}
	// Make sure you have a reference to System.Management or this won't compile
	//
	// Query code from https://32feet.codeplex.com/
	// Modified by Xander Soldaat
	static List<String> getComPorts ()
	{
		// The WMI query that makes it all happen
		const string QueryString = "SELECT Caption,PNPDeviceID FROM Win32_PnPEntity " +
			"WHERE ConfigManagerErrorCode = 0 AND " +
			"Caption LIKE 'Standard Serial over Bluetooth link (COM%' AND " +
			"PNPDeviceID LIKE '%&001653%'";

		SelectQuery WMIquery = new SelectQuery (QueryString);
		ManagementObjectSearcher WMIqueryResults = new ManagementObjectSearcher (WMIquery);
		if (WMIqueryResults != null) {
			List<String> comPorts = new List<String> ();
			Console.WriteLine ("The following NXTs were found on your system:");
			foreach (object result in WMIqueryResults.Get()) {
				ManagementObject mo = (ManagementObject)result;
				object captionObject = mo.GetPropertyValue ("Caption");
				object pnpIdObject = mo.GetPropertyValue ("PNPDeviceID");

				// Get the COM port name out of the Caption, requires a little search and replacing.
				string caption = captionObject.ToString ();
				string comPort = caption.Substring (caption.LastIndexOf ("(COM")).
                                    Replace ("(", string.Empty).Replace (")", string.Empty);

				// Extract the BT address from the PNPObjectID property
				string BTaddress = pnpIdObject.ToString ().Split ('&') [4].Substring (0, 12);
				comPorts.Add (comPort);
				Console.WriteLine ("COM Port: {0} ", comPort);
				Console.WriteLine ("BT Addr:  {0} ", BTaddress);
				Console.WriteLine ("");
			}
			return comPorts;
		} else {
			Console.WriteLine ("Error executing query");
			return null;
		}
	}

	static void connect ()
	{
		foreach (String port in getComPorts()) {
			try {
				ev3 = new Brick<Sensor, Sensor, Sensor, Sensor> (port);
				Console.WriteLine ("\tTrying to connect to NXT on " + port);
				ev3.Connection.Open ();
			} catch (ConnectionException e) {
				Console.WriteLine ("\tCould not connect on " + port + ": " + e.Message);
				continue;
			}
		}

	}
}
