using System.Collections.Generic;
using Newtonsoft.Json;
using PartialFoods.Services;

namespace PartialFoods.Services.OrderCommandServer
{
    public interface IEventEmitter
    {
        bool EmitOrderAcceptedEvent(OrderAcceptedEvent evt);
        bool EmitInventoryReservedEvent(InventoryReservedEvent evt);

        bool EmitOrderCanceledEvent(OrderCanceledEvent evt);
        bool EmitInventoryReleasedEvent(InventoryReleasedEvent evt);
    }
}