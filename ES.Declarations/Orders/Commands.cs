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
            IMessageContext Context
        ) : MessageContext(Context);

        public record SetOrderPlaced(
            string OrderId,
            IMessageContext Context
        ) : MessageContext(Context);


        public record CancelOrder(
            string OrderId,
            string Reason,
            IMessageContext Context
        ) : MessageContext(Context);

        public record AdjustProducts(
            string OrderId,
            OrderItemChange[] Changes,
            IMessageContext Context
        ) : MessageContext(Context);

    }

}