using ES.Core;
using Eventuous;

namespace ES.Declarations.Orders
{
    public static class Events
    {
        public const string AggregateName = "Order";
        [EventType($"V1.{AggregateName}.{nameof(OrderDrafted)}")]
        public record OrderDrafted(
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderCanceled)}")]
        public record OrderCanceled(
            string Reason,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderPlaced)}")]
        public record OrderPlaced(
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ItemsAdjusted)}")]
        public record ItemsAdjusted(
            OrderItemChange[] Changes,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ItemsAdjustingFialeded)}")]
        public record ItemsAdjustingFialeded(
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;
    }
}