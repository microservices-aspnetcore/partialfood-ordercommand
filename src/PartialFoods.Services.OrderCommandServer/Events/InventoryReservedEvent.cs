using Newtonsoft.Json;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class InventoryReservedEvent : InventoryEvent
    {
        private const string RESERVED_TOPIC = "inventoryreserved";

        public override string Topic()
        {
            return RESERVED_TOPIC;
        }
    }
}