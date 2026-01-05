using ES.Core;
using ES.Declarations.Orders;

namespace ES.Declarations.PlaceOrderSaga
{
    public static class Commands
    {
        public record Start(
            string SagaId,
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record MarkProductReserved(
            string SagaId,
            string ProductId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record MarkProductReservationFailed(
            string SagaId,
            string ProductId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
