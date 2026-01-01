using ES.Core;
using ES.Declarations;
using Eventuous;

namespace ES.Declarations.Orders
{
    public static class Events
    {
        [EventType("V1.OrderDrafted")]
        public record OrderDrafted(
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType("V1.OrderCanceled")]
        public record OrderCanceled(
            string Reason,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType("V1.OrderPlaced")]
        public record OrderPlaced(
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType("V1.ItemsAdjusted")]
        public record ItemsAdjusted(
            OrderItemChange[] Changes,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType("V1.ItemsAdjustingFialeded")]
        public record ItemsAdjustingFialeded(
            IMessageContext Context
        ) : MessageContext(Context);
    }

}