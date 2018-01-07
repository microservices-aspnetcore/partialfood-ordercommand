using System;
using Grpc.Core;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 3000;

            IEventEmitter kafkaEmitter = new KafkaEventEmitter();
            // TODO: this channel needs to use a service-discovery hostname
            Channel orderChannel = new Channel("127.0.0.1:3001", ChannelCredentials.Insecure);
            var orderClient = new OrderManagement.OrderManagementClient(orderChannel);

            Server server = new Server
            {
                Services = { OrderCommand.BindService(new OrderCommandImpl(kafkaEmitter, orderClient)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Orders Command RPC Service Listening on Port " + Port);
            Console.WriteLine("Press any key to stop");

            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
