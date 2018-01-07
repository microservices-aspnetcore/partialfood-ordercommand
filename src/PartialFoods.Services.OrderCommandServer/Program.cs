using System;
using System.IO;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var port = int.Parse(Configuration["service:port"]);

            IEventEmitter kafkaEmitter = new KafkaEventEmitter();
            // TODO: this channel needs to use a service-discovery hostname
            Channel orderChannel = new Channel("127.0.0.1:3001", ChannelCredentials.Insecure);
            var orderClient = new OrderManagement.OrderManagementClient(orderChannel);

            Server server = new Server
            {
                Services = { OrderCommand.BindService(new OrderCommandImpl(kafkaEmitter, orderClient)) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Orders Command RPC Service Listening on Port " + port);
            Console.WriteLine("Press any key to stop");

            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
