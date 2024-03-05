using Common;
using NATS.Client;
using NATS.Client.JetStream;
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

        private static IJetStream JetStream;
        private static IJetStreamManagement JetStreamManagement;

        const string subjectName = "eventsqueue";

        static void Main(string[] args)
        {
            Console.WriteLine("starting up...");

            NatsUrl = args != null && args.Length > 0 ? args[0] : Constants.DefaultNatsUrl;
            NatsConnection = Helpers.ConnectToNats(NatsUrl);
            if (NatsConnection == null)
                return;

            JetStream = NatsConnection.CreateJetStreamContext();
            JetStreamManagement = NatsConnection.CreateJetStreamManagementContext();

            string streamName = "migrationQueue";

            try
            {
                JetStreamManagement.DeleteStream(streamName);
            }
            catch { }

            JetStreamManagement.AddStream(StreamConfiguration.Builder()
                .WithName(streamName)
                .WithStorageType(StorageType.Memory)
                .WithSubjects(subjectName)
                .WithRetentionPolicy(RetentionPolicy.WorkQueue)
                .Build());

            const int defaultSubscribers = 2;
            Console.WriteLine($"starting {1} subscribers by default...");
            for(var i = 0; i < defaultSubscribers; ++i)
            {
                LaunchSubscriber();
            }

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
                case "pub":
                    Publish(string.Join(" ", splitInput.Skip(1)));
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

        private static void Publish(string message)
        {
            try
            {
                JetStream.Publish(subjectName, Encoding.UTF8.GetBytes(message), PublishOptions.Builder().WithTimeout(2000).Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }

            Console.WriteLine($"published: '{message}' to {subjectName}");
        }
    }
}
