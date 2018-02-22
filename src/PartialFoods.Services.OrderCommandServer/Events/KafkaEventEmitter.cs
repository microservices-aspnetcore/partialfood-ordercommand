using System;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using Newtonsoft.Json;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class KafkaEventEmitter : IEventEmitter
    {
        public bool Emit(DomainEvent evt)
        {
            try
            {
                var options = new KafkaOptions(new Uri("http://localhost:9092"));

                var router = new BrokerRouter(options);
                var client = new Producer(router);
                var topic = evt.Topic();

                string messageJson = JsonConvert.SerializeObject(evt);
                Console.WriteLine($"Emitting event {evt.EventID} on topic {topic}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to emit event {ex.ToString()}");
                return false;
            }
        }

        /* 
                public bool EmitInventoryReleasedEvent(InventoryReleasedEvent evt)
                {
                    try
                    {
                        var options = new KafkaOptions(new Uri("http://localhost:9092"));

                        var router = new BrokerRouter(options);
                        var client = new Producer(router);

                        string messageJson = JsonConvert.SerializeObject(evt);
                        Console.WriteLine($"Emitting Inventory Released Event {evt.EventID}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to emit inventory released event {ex.ToString()}");
                        return false;
                    }

                }
                public bool EmitOrderAcceptedEvent(OrderAcceptedEvent evt)
                {
                    try
                    {
                        evt.OrderID = Guid.NewGuid().ToString();
                        var options = new KafkaOptions(new Uri("http://localhost:9092"));

                        var router = new BrokerRouter(options);
                        var client = new Producer(router);

                        string messageJson = JsonConvert.SerializeObject(evt);
                        Console.WriteLine($"Emitting Order Accepted Event {evt.OrderID}");
                        client.SendMessageAsync(ORDERS_TOPIC, new[] { new Message(messageJson) }).Wait();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to emit order accepted event {ex.ToString()}");
                        return false;
                    }
                }

                public bool EmitOrderCanceledEvent(OrderCanceledEvent evt)
                {
                    try
                    {
                        var options = new KafkaOptions(new Uri("http://localhost:9092"));

                        var router = new BrokerRouter(options);
                        var client = new Producer(router);

                        string messageJson = JsonConvert.SerializeObject(evt);
                        Console.WriteLine($"Emitting order canceled event for order {evt.OrderID}");
                        client.SendMessageAsync(CANCELED_TOPIC, new[] { new Message(messageJson) }).Wait();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to emit order canceled event {ex.ToString()}");
                        return false;
                    }
                }

                public bool EmitInventoryReservedEvent(InventoryReservedEvent evt)
                {
                    try
                    {
                        evt.EventID = Guid.NewGuid().ToString();
                        var options = new KafkaOptions(new Uri("http://localhost:9092"));

                        var router = new BrokerRouter(options);
                        var client = new Producer(router);

                        string messageJson = JsonConvert.SerializeObject(evt);
                        Console.WriteLine($"Emitting Inventory Reserved Event for Order {evt.OrderID}, SKU {evt.SKU}");
                        client.SendMessageAsync(RESERVED_TOPIC, new[] { new Message(messageJson) }).Wait();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to emit inventory reserved event {ex.ToString()}");
                        return false;
                    }
                } */
    }
}