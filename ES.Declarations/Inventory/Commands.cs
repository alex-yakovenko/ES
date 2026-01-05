using ES.Core;

namespace ES.Declarations.Inventory
{
    public static class Commands
    {
        public record CreateInventoryItem(
            string ProductId,
            string Name,
            int InitialQuantity,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record ReserveProduct(
            string ProductId,
            int Quantity,
            string OrderId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record CancelProductReservation(
            string ProductId,
            string OrderId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record ReleaseProduct(
            string ProductId,
            int IncreaseQuantityBy,
            string OrderId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
