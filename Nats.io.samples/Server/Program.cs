using Common;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        private static IConnection NatsConnection = null;
        private static string NatsUrl = null;

        static void Main(string[] args)
        {
            Console.WriteLine("starting up...");

            NatsUrl = args != null && args.Length > 0 ? args[0] : Constants.DefaultNatsUrl;
            NatsConnection = Helpers.ConnectToNats(NatsUrl);
            if (NatsConnection == null)
                return;
        
            for (; ;)
            {
                var input = Console.ReadLine().ToLowerInvariant();

                if (input == "q" || input == "quit")
                {
                    Console.WriteLine("shutting down...");
                    return;
                }

                ProcessCommand(input);
            }
        }

        private static void ProcessCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
                return;

            var splitInput = input.Split(' '); 
            var firstWord = splitInput.First();

            switch (firstWord)
            {
                case "publish":
                    Publish(splitInput.Skip(1).ToArray());
                    break;

                case "launch":
                    Launch(splitInput.Skip(1).ToArray());
                    break;

                default:
                    Console.WriteLine($"unrecognised command '{firstWord}'");
                    break;
            }
        }

        private static void Launch(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("invalid arguments for launch command");
                return;
            }

            var arg = args[0];
        
            switch (arg)
            {
                case "subscriber":
                    LaunchSubscriber();
                    break;

                default:
                    Console.WriteLine($"unrecognised argument '{arg}'");
                    break;
            }
        }

        private static void LaunchSubscriber()
        {
            Console.WriteLine($"starting new subscriber...");

            Process.Start("Subscriber.exe", NatsUrl);
        }

        private static void Publish(string[] args)
        {
            if (args == null || args.Length != 2)
            {
                Console.WriteLine("invalid arguments for publish command");
                return;
            }

            var topic = args[0];
            var content = args[1];

            try
            {
                NatsConnection.Publish(topic, Encoding.UTF8.GetBytes(content));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }

            Console.WriteLine($"published: '{content}' to {topic}");
        }
    }
}
