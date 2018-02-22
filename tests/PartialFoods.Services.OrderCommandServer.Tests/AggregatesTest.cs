using System;
using System.Collections.Generic;
using PartialFoods.Services.OrderCommandServer.Events;
using Xunit;

namespace PartialFoods.Services.OrderCommandServer.Tests
{
    public class AggregatesTest
    {
        [Fact]
        public void Test_Order_AcceptProducesProductReserveEvents()
        {
            var tx = new OrderRequest
            {
                UserID = Guid.NewGuid().ToString(),
                CreatedOn = (ulong)DateTime.UtcNow.Ticks,
                TaxRate = 5
            };
            tx.LineItems.Add(new LineItem { SKU = "ABC123", UnitPrice = 12, Quantity = 1 });
            tx.LineItems.Add(new LineItem { SKU = "SOAP12", UnitPrice = 200, Quantity = 5 });

            var oa = new OrderAggregate("order-2-1");

            var productAggregates = FakeAggs();

            var evts = oa.Accept(tx, productAggregates);
            Assert.Equal(3, evts.Count);
            Assert.IsType<OrderAcceptedEvent>(evts[0]);
            Assert.IsType<InventoryReservedEvent>(evts[1]);
            Assert.IsType<InventoryReservedEvent>(evts[2]);
            var evt = (InventoryReservedEvent)evts[2];
            Assert.Equal<uint>(5, evt.Quantity);
        }
        [Fact]
        public void Test_Order_CancelProducesProductReleaseEvents()
        {
            OrderAggregate oa = new OrderAggregate("order-1-1");

            var productAggs = FakeAggs();
            var evts = oa.Cancel("tester", productAggs);
            Assert.Equal(3, evts.Count);
            Assert.IsType<OrderCanceledEvent>(evts[0]);
            Assert.IsType<InventoryReleasedEvent>(evts[1]);
            Assert.IsType<InventoryReleasedEvent>(evts[2]);
        }

        private Dictionary<String, ProductAggregate> FakeAggs()
        {
            var productAggs = new Dictionary<String, ProductAggregate>();

            var a1 = new ProductAggregate("ABC123");
            a1.Apply(new InventoryStockEvent
            {
                SKU = "ABC123",
                Quantity = 100,
                EventTime = (ulong)DateTime.UtcNow.Ticks,
            });
            a1.Apply(new InventoryReservedEvent
            {
                SKU = "ABC123",
                OrderID = "order-1-1",
                Quantity = 1,
                EventTime = (ulong)DateTime.UtcNow.Ticks,
            });

            var a2 = new ProductAggregate("SOAP12");
            a2.Apply(new InventoryStockEvent
            {
                SKU = "SOAP12",
                Quantity = 50,
                EventTime = (ulong)DateTime.UtcNow.Ticks,
            });
            productAggs["ABC123"] = a1;
            productAggs["SOAP12"] = a2;

            return productAggs;
        }
    }
}
