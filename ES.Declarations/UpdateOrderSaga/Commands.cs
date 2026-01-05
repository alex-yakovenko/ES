using ES.Core;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Commands
    {
        public record MarkProductReservedOrReleased(
            string SagaId,
            string ProductId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record MarkProductReservationFailed1(
            string SagaId,
            string ProductId,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record UpdateStatus(
            string SagaId,
            bool Success,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;

        public record StartUpdate(
            string SagaId,
            string OrderId,
            List<OrderItemChangeInfo> Changes,
            string TenantId,
            string? CorrelationId = null) : IMessageContext;
    }
}
