using ES.Core;
using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Events
    {
        [EventType($"V1.{nameof(Started)}")]
        public record Started(
            string SagaId,
            List<OrderItemChangeInfo> Changes,
            string OrderId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{nameof(OrderUpdateStatus)}")]
        public record OrderUpdateStatus(bool Success, string TenantId, string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{nameof(ReservationFailed)}")]
        public record ReservationFailed(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{nameof(ProductReservedOrReleased)}")]
        public record ProductReservedOrReleased(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{nameof(AllProductsReserved)}")]
        public record AllProductsReserved(
            string OrderId,
            OrderItemChangeInfo[] Changes,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{nameof(ReservationsCancellingNeeded)}")]
        public record ReservationsCancellingNeeded(
            string OrderId,
            string[] ProductIdsToCancelReservation,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
