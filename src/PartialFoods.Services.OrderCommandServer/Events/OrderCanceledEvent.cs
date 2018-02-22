using Newtonsoft.Json;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class OrderCanceledEvent : OrderEvent
    {
        private const string CANCELED_TOPIC = "canceledorders";

        public override string Topic()
        {
            return CANCELED_TOPIC;
        }
    }
}