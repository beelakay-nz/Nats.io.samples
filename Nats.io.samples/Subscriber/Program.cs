using Common;
using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber
{
    internal class Program
    {
        private static IConnection NatsConnection = null;

        private static IJetStream JetStream;
        private static IJetStreamManagement JetStreamManagement;

        private static Random Rand = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("starting up...");

            var natsUrl = args != null && args.Length > 0 ? args[0] : Constants.DefaultNatsUrl;
            NatsConnection = Helpers.ConnectToNats(natsUrl);
            if (NatsConnection == null)
                return;

            JetStream = NatsConnection.CreateJetStreamContext();
            JetStreamManagement = NatsConnection.CreateJetStreamManagementContext();

            string streamName = "migrationQueue";

            var subOpts = PullSubscribeOptions.Builder()
                .WithDurable("myConsumer")
                .WithStream(streamName)
                .Build();

            // pull sub - only one subscriber will receive message
            // push sub - all subscribers receive all messages
            var isPull = true;

            if (isPull)
            {
                var pullSub  = JetStream.PullSubscribe("eventsqueue", subOpts);

                for(; ;)
                {
                    Console.WriteLine("pulling from nats server...");
                    pullSub.Pull(10); 

                    for(; ;)
                    {
                        var msg = pullSub.NextMessage();

                        if (msg == null)
                            break;

                        await OnMessageReceived(msg);
                    }

                    await Task.Delay(100);
                }
            }
            else
            {
                var pushSub = JetStream.PushSubscribeAsync("eventsqueue", OnPushMessageReceived, false);

                Console.ReadLine();
            }
        }

        private static void OnPushMessageReceived(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            Task.Run(async () => await OnMessageReceived(args.Message));
        }

        private static async Task OnMessageReceived(Msg message)
        {
            Console.WriteLine($"received: '{message}' on {message.Subject}. is jetstream: {message.IsJetStream}");

            // message.InProgress();

            Console.WriteLine($"processing message at {DateTime.UtcNow}...");
            await Task.Delay(TimeSpan.FromSeconds(3));

            if (Rand.Next(10) < 5)
            {
                Console.WriteLine($"failed to process message");
                message.Nak();
            }
            else
            {
                Console.WriteLine($"successfully processed message");
               message.Ack();
            }
        }
    }
}
