using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class KafkaEventEmitter : IEventEmitter
    {
        private Dictionary<string, object> config;
        private ILogger logger;

        public KafkaEventEmitter(Dictionary<string, object> config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
            logger.LogInformation($"Kafka event emitter configured for broker list {config["bootstrap.servers"]}");
        }

        public bool Emit(DomainEvent evt)
        {
            try
            {
                var topic = evt.Topic();
                string messageJson = JsonConvert.SerializeObject(evt);
                logger.LogInformation($"Emitting event {evt.GetType().FullName} ({evt.EventID}) on topic {topic}.");
                using (var producer = new Producer<Null, string>(this.config, null, new StringSerializer(Encoding.UTF8)))
                {
                    var result = producer.ProduceAsync(topic, null, messageJson).Result;
                    if (result.Error != null)
                    {
                        // log the error
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to emit event {ex.ToString()}");
                return false;
            }
        }
    }
}