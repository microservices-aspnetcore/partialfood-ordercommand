using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public abstract class OrderEvent : DomainEvent
    {
        [JsonProperty("order_id")]
        public string OrderID;

        [JsonProperty("created_on")]
        public ulong CreatedOn;

        [JsonProperty("user_id")]
        public string UserID;
    }

    public class OrderAcceptedEvent : OrderEvent
    {
        private const string ORDERS_TOPIC = "orders";

        public override string Topic()
        {
            return ORDERS_TOPIC;
        }

        [JsonProperty("tax_rate")]
        public uint TaxRate;

        [JsonProperty("line_items")]
        public ICollection<EventLineItem> LineItems;

        public static OrderAcceptedEvent FromProto(OrderRequest tx, string orderID)
        {
            var evt = new OrderAcceptedEvent
            {
                TaxRate = tx.TaxRate,
                CreatedOn = tx.CreatedOn,
                UserID = tx.UserID,
                EventID = Guid.NewGuid().ToString(),
                OrderID = orderID
            };
            evt.LineItems = new List<EventLineItem>();
            foreach (LineItem li in tx.LineItems)
            {
                evt.LineItems.Add(new EventLineItem
                {
                    SKU = li.SKU,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice
                });
            }

            return evt;
        }
    }

    public class EventLineItem
    {
        [JsonProperty("sku")]
        public string SKU;

        [JsonProperty("unit_price")]
        public uint UnitPrice;

        [JsonProperty("quantity")]
        public uint Quantity;
    }
}