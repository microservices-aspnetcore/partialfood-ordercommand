using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartialFoods.Services;
using PartialFoods.Services.OrderCommandServer.Events;
using Grpc.Reflection.V1Alpha;
using Grpc.Reflection;
using System.Collections.Generic;

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

            string brokerList = Configuration["kafkaclient:brokerlist"];
            var config = new Dictionary<string, object>
            {
                { "group.id", "order-command"},
                { "enable.auto.commit", "false"},
                { "bootstrap.servers", brokerList }
            };

            IEventEmitter kafkaEmitter = new KafkaEventEmitter(config, loggerFactory.CreateLogger<KafkaEventEmitter>());
            // TODO: this channel needs to use a service-discovery hostname
            Channel orderChannel = new Channel($"{Configuration["orderclient:hostname"]}:{Configuration["orderclient:port"]}",
                 ChannelCredentials.Insecure);
            Channel inventoryChannel = new Channel($"{Configuration["inventoryclient:hostname"]}:{Configuration["inventoryclient:port"]}",
                ChannelCredentials.Insecure);
            logger.LogInformation($"Configured gRPC channel for Order Management client: {orderChannel.ResolvedTarget}");
            logger.LogInformation($"Configured gRPC channel for Inventory Management client: {inventoryChannel.ResolvedTarget}");

            var orderClient = new OrderManagement.OrderManagementClient(orderChannel);
            var inventoryClient = new InventoryManagement.InventoryManagementClient(inventoryChannel);

            var refImpl = new ReflectionServiceImpl(
                ServerReflection.Descriptor, OrderCommand.Descriptor);

            var rpcLogger = loggerFactory.CreateLogger<OrderCommandImpl>();
            Server server = new Server
            {
                Services = { OrderCommand.BindService(new OrderCommandImpl(rpcLogger, kafkaEmitter, orderClient, inventoryClient)),
                              ServerReflection.BindService(refImpl) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };
            server.Start();

            logger.LogInformation("Orders Command gRPC Service Listening on Port " + port);

            // Keep the process alive without consuming CPU cycles
            mre.WaitOne();
        }
    }
}
