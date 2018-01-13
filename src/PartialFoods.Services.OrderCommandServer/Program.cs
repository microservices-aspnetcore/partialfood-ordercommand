using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer
{
    class Program
    {
        private static ManualResetEvent mre = new ManualResetEvent(false);

        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables();

            Configuration = builder.Build();

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            ILogger logger = loggerFactory.CreateLogger<Program>();

            var port = int.Parse(Configuration["service:port"]);

            IEventEmitter kafkaEmitter = new KafkaEventEmitter();
            // TODO: this channel needs to use a service-discovery hostname
            Channel orderChannel = new Channel($"{Configuration["orderclient:hostname"]}:{Configuration["orderclient:port"]}",
                 ChannelCredentials.Insecure);
            logger.LogInformation($"Configured gRPC channel for Order Management client: {orderChannel.ResolvedTarget}");
            var orderClient = new OrderManagement.OrderManagementClient(orderChannel);

            Server server = new Server
            {
                Services = { OrderCommand.BindService(new OrderCommandImpl(kafkaEmitter, orderClient)) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };
            server.Start();

            logger.LogInformation("Orders Command RPC Service Listening on Port " + port);

            // Keep the process alive without consuming CPU cycles
            mre.WaitOne();
        }
    }
}
