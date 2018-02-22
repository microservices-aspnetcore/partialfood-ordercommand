using Newtonsoft.Json;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public abstract class InventoryEvent : DomainEvent
    {
        [JsonProperty("sku")]
        public string SKU { get; set; }

        [JsonProperty("quantity")]
        public uint Quantity { get; set; }

        [JsonProperty("order_id")]
        public string OrderID { get; set; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("event_time")]
        public ulong EventTime { get; set; }

    }

    public class InventoryReleasedEvent : InventoryEvent
    {
        private const string RELEASED_TOPIC = "inventoryreleased";

        public override string Topic()
        {
            return RELEASED_TOPIC;
        }
    }

    public class InventoryStockEvent : InventoryEvent
    {
        private const string STOCK_TOPIC = "inventorystock";

        public override string Topic()
        {
            return STOCK_TOPIC;
        }
    }
}