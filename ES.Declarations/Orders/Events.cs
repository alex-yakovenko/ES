using ES.Core;
using ES.Declarations;
using Eventuous;

namespace ES.Declarations.Orders
{
    public static class Events
    {
        [EventType($"V1.{nameof(OrderDrafted)}")]
        public record OrderDrafted(
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType($"V1.{nameof(OrderCanceled)}")]
        public record OrderCanceled(
            string Reason,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType($"V1.{nameof(OrderPlaced)}")]
        public record OrderPlaced(
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType($"V1.{nameof(ItemsAdjusted)}")]
        public record ItemsAdjusted(
            OrderItemChange[] Changes,
            IMessageContext Context
        ) : MessageContext(Context);

        [EventType($"V1.{nameof(ItemsAdjustingFialeded)}")]
        public record ItemsAdjustingFialeded(
            IMessageContext Context
        ) : MessageContext(Context);
    }

}