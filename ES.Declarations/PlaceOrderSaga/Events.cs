using ES.Core;
using ES.Declarations;
using Eventuous;

namespace ES.Declarations.PlaceOrderSaga
{
    public static class Events
    {
        [EventType($"V1.{nameof(Started)}")]
        public record Started(string SagaId, string OrderId, string CustomerId, DateOnly Date, List<OrderItem> Items, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(RezervationSent)}")]
        public record RezervationSent(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(ProductReserved)}")]
        public record ProductReserved(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(ProductReservationFailed)}")]
        public record ProductReservationFailed(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(OrderCanBePlaced)}")]
        public record OrderCanBePlaced(string OrderId, IMessageContext Context) : MessageContext(Context);

        [EventType($"V1.{nameof(OrderNeedsToBeCancelled)}")]
        public record OrderNeedsToBeCancelled(string OrderId, List<string> ProductIds, IMessageContext Context) : MessageContext(Context);
    }
}
