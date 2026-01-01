using ES.Core;

namespace ES.Declarations.UpdateOrderSaga
{
    public class Commands
    {
        public record MarkProductReservedOrReleased(string SagaId, string ProductId, 
            IMessageContext Context) : MessageContext(Context);

        public record MarkProductReservationFailed(string SagaId, string ProductId, 
            IMessageContext Context) : MessageContext(Context);

        public record UpdateStatus(string SagaId, bool Success, 
            IMessageContext Context) : MessageContext(Context);

        public record Start(string SagaId, string OrderId, List<OrderItemChange> Changes, 
            IMessageContext Context) : MessageContext(Context);
    }
}
