using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core.Utils;
using grpc = global::Grpc.Core;
using PartialFoods.Services;
using Google.Protobuf; // required for byte array extension

namespace PartialFoods.Services.OrderCommandServer
{
    public class OrderCommandImpl : OrderCommand.OrderCommandBase
    {
        private IEventEmitter eventEmitter;
        private OrderManagement.OrderManagementClient orderManagementClient;

        public OrderCommandImpl(IEventEmitter emitter, OrderManagement.OrderManagementClient orderManagementClient)
        {
            eventEmitter = emitter;
            this.orderManagementClient = orderManagementClient;
        }

        public override Task<CancelOrderResponse> CancelOrder(CancelOrderRequest request, grpc::ServerCallContext context)
        {
            Console.WriteLine("Handling Order cancellation request");


            var result = new CancelOrderResponse();
            try
            {
                var exists = orderManagementClient.OrderExists(new GetOrderRequest { OrderID = request.OrderID });
                if (!exists.Exists)
                {
                    result.Canceled = false;
                    return Task.FromResult(result);
                }
                var originalOrder = orderManagementClient.GetOrder(new GetOrderRequest { OrderID = request.OrderID });

                var evt = new OrderCanceledEvent
                {
                    OrderID = request.OrderID,
                    UserID = request.UserID,
                    CreatedOn = (ulong)DateTime.UtcNow.Ticks,
                    ActivityID = Guid.NewGuid().ToString(),
                };

                if (eventEmitter.EmitOrderCanceledEvent(evt))
                {
                    result.ConfirmationCode = Guid.NewGuid().ToString();
                    result.Canceled = true;
                    foreach (var li in originalOrder.LineItems)
                    {
                        eventEmitter.EmitInventoryReleasedEvent(new InventoryReleasedEvent
                        {
                            SKU = li.SKU,
                            ReleasedOn = (ulong)DateTime.UtcNow.Ticks,
                            Quantity = li.Quantity,
                            OrderID = request.OrderID,
                            UserID = request.UserID,
                            EventID = Guid.NewGuid().ToString()
                        });
                    }
                }
                else
                {
                    result.Canceled = false;
                }
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to handle order cancellation {ex.ToString()}");
                result.Canceled = false;
                return Task.FromResult(result);
            }
        }

        public override Task<OrderResponse> SubmitOrder(OrderRequest request, grpc::ServerCallContext context)
        {
            Console.WriteLine("Handling Order Request Submission");
            var response = new OrderResponse();

            if (!isValidRequest(request))
            {
                response.Accepted = false;
                return Task.FromResult(response);
            }

            var evt = OrderAcceptedEvent.FromProto(request);

            if (eventEmitter.EmitOrderAcceptedEvent(evt))
            {
                foreach (var li in evt.LineItems)
                {
                    var reservedEvent = new InventoryReservedEvent
                    {
                        OrderID = evt.OrderID,
                        ReservedOn = (ulong)DateTime.UtcNow.Ticks,
                        SKU = li.SKU,
                        Quantity = li.Quantity,
                        UserID = evt.UserID
                    };
                    if (!eventEmitter.EmitInventoryReservedEvent(reservedEvent))
                    {
                        response.Accepted = false;
                        return Task.FromResult(response);
                    }
                }
                response.OrderID = evt.OrderID;
                response.Accepted = true;
            }
            else
            {
                response.Accepted = false;
            }
            return Task.FromResult(response);
        }

        private bool isValidRequest(OrderRequest request)
        {
            if (request.LineItems.Count == 0)
            {
                return false;
            }
            if (request.TaxRate > 50)
            {
                return false;
            }

            return true;
        }
    }
}