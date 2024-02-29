using Common;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Console.WriteLine("listening...");
            Console.ReadLine();
        }
    }
}
