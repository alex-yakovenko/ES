using ES.Core;
using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Events
    {
        [EventType($"V1.{nameof(Started)}")]
        public record Started(string SagaId, List<OrderItemChange> Changes, string OrderId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(OrderUpdateStatus)}")]
        public record OrderUpdateStatus(bool Success, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(ReservationFailed)}")]
        public record ReservationFailed(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(ProductReservedOrReleased)}")]
        public record ProductReservedOrReleased(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(AllProductsReserved)}")]
        public record AllProductsReserved(string OrderId, OrderItemChange[] Changes, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(ReservationsCancellingNeeded)}")]
        public record ReservationsCancellingNeeded(string OrderId, string[] ProductIdsToCancelReservation, IMessageContext Context) : MessageContext(Context);
    }
}
