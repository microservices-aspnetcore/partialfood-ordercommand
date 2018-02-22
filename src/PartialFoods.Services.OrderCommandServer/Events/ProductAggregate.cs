using System;
using System.Collections.Generic;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class ProductAggregate : IAggregate<InventoryEvent>
    {
        private string sku;
        private ulong version;
        private ulong quantity;

        public ProductAggregate(string sku)
        {
            this.sku = sku;
            this.quantity = 0; // initial quantity comes from an event
            this.version = 1;
        }
        public ulong Version
        {
            get
            {
                return this.version;
            }
        }

        public ulong Quantity
        {
            get
            {
                return this.quantity;
            }
        }

        public string SKU
        {
            get
            {
                return this.sku;
            }
        }

        public IList<InventoryEvent> Reserve(string orderID, string userID, uint quantity)
        {
            var evts = new List<InventoryEvent>();

            if (quantity > this.quantity)
            {
                throw new ArgumentOutOfRangeException("quantity", "Cannot reserve more than on hand quantity");
            }

            var reservedEvent = new InventoryReservedEvent
            {
                OrderID = orderID,
                EventTime = (ulong)DateTime.UtcNow.Ticks,
                SKU = this.sku,
                Quantity = quantity,
                UserID = userID,
                EventID = Guid.NewGuid().ToString()
            };
            evts.Add(reservedEvent);
            return evts;
        }

        public IList<InventoryEvent> Release(string orderID, string userID, uint quantity)
        {
            var evts = new List<InventoryEvent>();

            var releasedEvent = new InventoryReleasedEvent
            {
                SKU = this.sku,
                EventTime = (ulong)DateTime.UtcNow.Ticks,
                Quantity = quantity,
                OrderID = orderID,
                UserID = userID,
                EventID = Guid.NewGuid().ToString()
            };
            evts.Add(releasedEvent);
            return evts;
        }

        public void Apply(InventoryEvent evt)
        {
            this.version += 1;
            if (evt is InventoryReleasedEvent)
            {
                this.quantity += evt.Quantity;
            }
            else if (evt is InventoryReservedEvent)
            {
                this.quantity -= evt.Quantity;
            }
            else if (evt is InventoryStockEvent)
            {
                this.quantity += evt.Quantity;
            }
        }

        public void ApplyAll(IList<InventoryEvent> evts)
        {
            foreach (var evt in evts)
            {
                this.Apply(evt);
            }
        }

        public void ApplyAll(IList<global::PartialFoods.Services.Activity> activities)
        {
            foreach (var activity in activities)
            {
                this.Apply(ToEvent(activity));
            }
        }

        private InventoryEvent ToEvent(global::PartialFoods.Services.Activity activity)
        {
            InventoryEvent evt = null;

            if (activity.ActivityType == PartialFoods.Services.ActivityType.Released)
            {
                evt = new InventoryReleasedEvent();
            }
            else if (activity.ActivityType == PartialFoods.Services.ActivityType.Reserved)
            {
                evt = new InventoryReservedEvent();
            }
            else if (activity.ActivityType == PartialFoods.Services.ActivityType.Stockadd)
            {
                evt = new InventoryStockEvent();
            }

            if (evt != null)
            {
                evt.SKU = activity.SKU;
                evt.EventTime = activity.Timestamp;
                evt.UserID = "";
                evt.Quantity = activity.Quantity;
                evt.OrderID = activity.OrderID;
                evt.EventID = activity.ActivityID;
            }

            return evt;
        }
    }
}