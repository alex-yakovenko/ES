using ES.Core;
using ES.Declarations;

namespace ES.Declarations.Orders
{
    public static class Commands
    {
        public record DraftOrder(
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        public record SetOrderPlaced(
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;


        public record CancelOrder(
            string OrderId,
            string Reason,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        public record AdjustProducts(
            string OrderId,
            OrderItemChange[] Changes,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;
    }
}