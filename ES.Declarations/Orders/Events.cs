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
            string OrderId,
            string Reason,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(OrderPlaced)}")]
        public record OrderPlaced(
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ItemsAdjusted)}")]
        public record ItemsAdjusted(
            string OrderId,
            OrderItemChange[] Changes,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ItemsAdjustingFialeded)}")]
        public record ItemsAdjustingFialeded(
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;
    }
}