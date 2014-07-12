using System;
using System.Threading;
using System.Management;
using System.Collections.Generic;
using System.IO;
using MonoBrick.EV3;
using MonoBrick;

public static class Program
{
    private static Brick<Sensor, Sensor, Sensor, Sensor> ev3 = null;

    static void Main(string[] args)
    {
        Console.OpenStandardOutput();
        Console.OpenStandardInput();
        Console.WriteLine("Starting C# backend");
		String input = "";
        while (true)
        {
            input = Console.In.ReadLine();
            if (input.Equals("exit") || input.Equals("q"))
            {
                Console.WriteLine("Exiting C# backend.");
                break;
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
					SendInt("command", int.Parse(cmd[0]));
                    if (cmd.Length == 2)
                    {
                        SendFloat("operand", float.Parse(cmd[1]));
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid arg");
                }
            }
        }
    }

	private static void SendInt(String mailbox, int value)
	{
		Console.WriteLine("Sending command #" + value);
		Console.WriteLine("command bytes:" + BitConverter.ToString(BitConverter.GetBytes(value)));
		if (ev3 == null)
			return;
		ev3.Mailbox.Send(mailbox, BitConverter.GetBytes(value));
	}

	private static void SendFloat(String mailbox, float value)
	{
		Console.WriteLine("\tOperand " + value);
		Console.WriteLine("operand bytes:" + BitConverter.ToString(BitConverter.GetBytes(value)));
		if (ev3 == null)
			return;
		ev3.Mailbox.Send(mailbox, BitConverter.GetBytes(value));
	}
}
