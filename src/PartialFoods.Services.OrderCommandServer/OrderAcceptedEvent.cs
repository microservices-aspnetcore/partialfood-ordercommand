using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer
{
    public class OrderAcceptedEvent
    {
        [JsonProperty("order_id")]
        public string OrderID;

        [JsonProperty("created_on")]
        public ulong CreatedOn;

        [JsonProperty("user_id")]
        public string UserID;

        [JsonProperty("tax_rate")]
        public uint TaxRate;

        [JsonProperty("line_items")]
        public ICollection<EventLineItem> LineItems;

        public static OrderAcceptedEvent FromProto(OrderRequest tx)
        {
            var evt = new OrderAcceptedEvent
            {
                TaxRate = tx.TaxRate,
                CreatedOn = tx.CreatedOn,
                UserID = tx.UserID
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