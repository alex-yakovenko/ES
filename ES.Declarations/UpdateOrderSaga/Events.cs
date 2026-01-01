using ES.Core;
using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Events
    {
        [EventType("V1.Started")]
        public record Started(string SagaId, List<OrderItemChange> Changes, string OrderId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.OrderUpdateStatus")]
        public record OrderUpdateStatus(bool Success, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.ReservationFailed")]
        public record ReservationFailed(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.ProductReservedOrReleased")]
        public record ProductReservedOrReleased(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.SetResult")]
        public record SetResult(string Result, string OrderId, string[] ProductIdsToCancelReservations, IMessageContext Context) : MessageContext(Context);
    }
}
