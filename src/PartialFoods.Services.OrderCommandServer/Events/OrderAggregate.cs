using System;
using System.Linq;
using System.Collections.Generic;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public class OrderAggregate : IAggregate<OrderEvent>
    {
        private ulong version;
        private string orderID;
        private OrderStatus status;


        public OrderAggregate(string orderID)
        {
            this.orderID = orderID;
            this.status = OrderStatus.None;
        }

        public OrderStatus Status
        {
            get
            {
                return this.status;
            }
        }

        public string OrderID
        {
            get
            {
                return this.orderID;
            }
        }

        public ulong Version
        {
            get
            {
                return this.version;
            }
        }

        public IList<DomainEvent> Accept(OrderRequest request, Dictionary<String, ProductAggregate> productAggregates)
        {
            var evts = new List<DomainEvent>();

            var evt = OrderAcceptedEvent.FromProto(request, this.OrderID);
            evts.Add(evt);

            foreach (string sku in productAggregates.Keys)
            {
                uint quantity = request.LineItems.ToArray().FirstOrDefault(li => li.SKU == sku).Quantity;
                evts.AddRange(productAggregates[sku].Reserve(this.orderID, request.UserID, quantity));
            }
            return evts;
        }

        public IList<DomainEvent> Cancel(string userID, Dictionary<String, ProductAggregate> productAggregates)
        {
            var evts = new List<DomainEvent>();
            var evt = new OrderCanceledEvent
            {
                OrderID = this.orderID,
                UserID = userID,
                CreatedOn = (ulong)DateTime.UtcNow.Ticks,
                EventID = Guid.NewGuid().ToString(),
            };
            evts.Add(evt);

            foreach (string sku in productAggregates.Keys)
            {
                evts.AddRange(productAggregates[sku].Release(this.orderID, userID, 1)); // TODO: use real quantity
            }

            return evts;
        }

        public void ApplyAll(IList<OrderEvent> evts)
        {
            foreach (var evt in evts)
            {
                this.Apply(evt);
            }
        }

        public void Apply(OrderEvent evt)
        {
            this.version += 1;
            if (evt is OrderAcceptedEvent)
            {
                this.status = OrderStatus.Accepted;
            }
            else if (evt is OrderCanceledEvent)
            {
                this.status = OrderStatus.Canceled;
            }
        }
    }

    public enum OrderStatus
    {
        None,
        Accepted,
        Canceled,
    }
}