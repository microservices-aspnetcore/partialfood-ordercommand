using System;
using Grpc.Core;
using PartialFoods.Services;

namespace PartialFoods.SampleCommandClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: this channel needs to use a proper service discovery host name
            // as well as injectable environment variable override
            Channel channel = new Channel("127.0.0.1:3000", ChannelCredentials.Insecure);

            var client = new OrderCommand.OrderCommandClient(channel);

            var tx = new OrderRequest
            {
                UserID = Guid.NewGuid().ToString(),
                CreatedOn = (ulong)DateTime.UtcNow.Ticks,
                TaxRate = 5
            };
            tx.LineItems.Add(new LineItem { SKU = "ABC123", UnitPrice = 12, Quantity = 1 });
            tx.LineItems.Add(new LineItem { SKU = "FYI555", UnitPrice = 200, Quantity = 5 });

            var response = client.SubmitOrder(tx);
            Console.WriteLine("Order Accepted - " + response.Accepted + ", OrderID - " + response.OrderID);
        }
    }
}
