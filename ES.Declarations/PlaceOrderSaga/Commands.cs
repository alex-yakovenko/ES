using ES.Core;
using ES.Declarations;

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
            IMessageContext Context) : MessageContext(Context);

        public record MarkProductReserved(
            string SagaId, 
            string ProductId, 
            IMessageContext Context) : MessageContext(Context);        
        
        public record MarkProductReservationFailed(
            string SagaId, 
            string ProductId, 
            IMessageContext Context) : MessageContext(Context);
    }
}
