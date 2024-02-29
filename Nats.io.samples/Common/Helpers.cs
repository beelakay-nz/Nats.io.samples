using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Helpers
    {
        public static IConnection ConnectToNats(string url)
        {
            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = url;

            Console.WriteLine("establishing connection to nats...");

            try
            {
                ConnectionFactory cf = new ConnectionFactory();
                var conn = cf.CreateConnection(opts);

                Console.WriteLine("connected to nats server!");

                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine("unable to connect to nats server!");
                Console.WriteLine($"error: {ex.Message}");

                return null;
            }  
        }
    }
}
