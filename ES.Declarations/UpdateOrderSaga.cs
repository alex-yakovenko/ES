using ES.Core;

using System;
using System.Collections.Generic;
using System.Text;

namespace ES.Declarations
{
    public class UpdateOrderSaga : Saga
    {
        public const string Stream = "UpdateOrder";

        public override string StreamType => Stream;

        public string OrderId { get; set; } = "";
        public string? Result { get; set; }
        public bool? OrderUpdateSuccess { get; set; }
        public List<OrderChangeWithFlafs> Changes { get; set; } = [];

        public void Apply(Events.Started evt)
        {
            Changes.AddRange(evt
                .Changes
                .Select(x => new OrderChangeWithFlafs(
                    x.ProductId, 
                    x.AdjustQuantityBy
                )
                {
                    WaitsForReservation = evt
                        .ProductsToWaitForReservations
                        .Contains(x.ProductId)
                }));
            OrderId = evt .OrderId;
            TenantId = evt.TenantId;
        }


        public void Apply(Events.ProductReservedOrReleased evt)
        {
            Changes.First(x => x.ProductId == evt.ProductId).ReservedSuccessfuly = true;
        }

        public void Apply(Events.RezervationFailed evt)
        {
            Changes.First(x => x.ProductId == evt.ProductId).FailedToReserve = true;
        }

        public void Apply(Events.SetResult evt)
        {
            Result = evt.Result;
        }

        public void Apply(Events.OrderUpdateStatus evt)
        {
            OrderUpdateSuccess = evt.Success;
        }

        public class Events
        {
            public record Started(string AggregateId, List<OrderItemChange> Changes, string OrderId, List<string> ProductsToWaitForReservations) : EsEvent(AggregateId, Stream);

            public record OrderUpdateStatus(string AggregateId, bool Success) : EsEvent(AggregateId, Stream);
            public record RezervationFailed(string AggregateId, string ProductId) : EsEvent(AggregateId, Stream);
            public record ProductReservedOrReleased(string AggregateId, string ProductId) : EsEvent(AggregateId, Stream);
            public record SetResult(string AggregateId, string Result) : EsEvent(AggregateId, Stream);
        }

        public class Commands 
        {
            public record Start(string AggregateId, string OrderId, List<OrderItemChange> Changes) : EsCommand<UpdateOrderSaga>(AggregateId);
        }

        public record OrderItemChange(string ProductId, int AdjustQuantityBy);

        public record OrderChangeWithFlafs(string ProductId, int AdjustQuantityBy)
        {
            public bool WaitsForReservation { get; set; }
            public bool ReservedSuccessfuly { get; set; }
            public bool FailedToReserve { get; set; }
        };

    }
}
