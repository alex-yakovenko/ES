using ES.Core;
using ES.Declarations;
using Eventuous;
using System;

namespace ES.Declarations.Inventory
{
    public static class Events
    {
        [EventType($"V1.{nameof(InventoryItemCreated)}")]
        public record InventoryItemCreated(
            string Code,
            int InitialQuantity,
            IMessageContext context
        ): MessageContext(context);

        [EventType($"V1.{nameof(ProductReserved)}")]
        public record ProductReserved(
            string ProductId,
            int Quantity,
            string OrderId,
            IMessageContext context
        ) : MessageContext(context);

        [EventType($"V1.{nameof(ProductReservationFailed)}")]
        public record ProductReservationFailed(
            string ProductId,
            int Quantity,
            string OrderId,
            IMessageContext context
        ) : MessageContext(context);

        [EventType($"V1.{nameof(ProductReservationCanceled)}")]
        public record ProductReservationCanceled(
            string OrderId, 
            IMessageContext context
        ) : MessageContext(context);

        [EventType($"V1.{nameof(ProductReleased)}")]
        public record ProductReleased(
            string ProductId,
            int Quantity,
            string OrderId,
            IMessageContext context
        ) : MessageContext(context);
    }
}
