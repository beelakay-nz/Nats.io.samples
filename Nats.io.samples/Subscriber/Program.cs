using Common;
using NATS.Client;
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

        static void Main(string[] args)
        {
            Console.WriteLine("starting up...");

            var natsUrl = args != null && args.Length > 0 ? args[0] : Constants.DefaultNatsUrl;
            NatsConnection = Helpers.ConnectToNats(natsUrl);
            if (NatsConnection == null)
                return;

            IAsyncSubscription subAsync = NatsConnection.SubscribeAsync(">", OnMessageReceived);

            Console.WriteLine("listening...");
            Console.ReadLine();
        }

        private static void OnMessageReceived(object sender, MsgHandlerEventArgs args)
        {
            var message = Encoding.UTF8.GetString(args.Message.Data);

            Console.WriteLine($"received: '{message}' on {args.Message.Subject}");
        }
    }
}
