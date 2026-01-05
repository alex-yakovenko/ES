using ES.Core;
using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Events
    {
        public const string AggregateName = "UpdateOrderSaga";

        [EventType($"V1.{AggregateName}.{nameof(Started)}")]
        public record Started(
            string SagaId,
            List<OrderItemChangeInfo> Changes,
            string OrderId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderUpdateStatus)}")]
        public record OrderUpdateStatus(bool Success, string TenantId, string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ReservationFailed)}")]
        public record ReservationFailed(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReservedOrReleased)}")]
        public record ProductReservedOrReleased(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(AllProductsReserved)}")]
        public record AllProductsReserved(
            string OrderId,
            OrderItemChangeInfo[] Changes,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ReservationsCancellingNeeded)}")]
        public record ReservationsCancellingNeeded(
            string OrderId,
            string[] ProductIdsToCancelReservation,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
