using ES.Core;

namespace ES.Declarations.Inventory
{
    public static class Commands
    {
        public record CreateInventoryItem(string ProductId, string Name, int InitialQuantity, IMessageContext context) : MessageContext(context);

        public record ReserveProduct(string ProductId, int Quantity, string OrderId, IMessageContext context) : MessageContext(context);

        public record CancelProductReservation(string ProductId, string OrderId, IMessageContext context) : MessageContext(context);

        public record ReleaseProduct(string ProductId, int IncreaseQuantityBy, string OrderId, IMessageContext context) : MessageContext(context);
    }
}
