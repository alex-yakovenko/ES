using ES.Core;
using ES.Declarations.Orders;
using Eventuous;

namespace ES.Declarations.PlaceOrderSaga
{
    public static class Events
    {
        public const string AggregateName = "PlaceOrderSaga";

        [EventType($"V1.{AggregateName}.{nameof(Started)}")]
        public record Started(
            string SagaId,
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(RezervationSent)}")]
        public record RezervationSent(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReserved)}")]
        public record ProductReserved(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReservationFailed)}")]
        public record ProductReservationFailed(string ProductId, string TenantId, string? CorrelationId = null)
            : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderCanBePlaced)}")]
        public record OrderCanBePlaced(string OrderId, string TenantId, string? CorrelationId = null) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderNeedsToBeCancelled)}")]
        public record OrderNeedsToBeCancelled(
            string OrderId,
            List<string> ProductIds,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
