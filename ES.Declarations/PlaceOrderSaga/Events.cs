using ES.Core;
using ES.Declarations;
using Eventuous;

namespace ES.Declarations.PlaceOrderSaga
{
    public static class Events
    {
        [EventType("V1.Started")]
        public record Started(string SagaId, string OrderId, string CustomerId, DateOnly Date, List<OrderItem> Items, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.RezervationSent")]
        public record RezervationSent(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.ProductReserved")]
        public record ProductReserved(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.ProductReservationFailed")]
        public record ProductReservationFailed(string ProductId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.OrderCanBePlaced")]
        public record OrderCanBePlaced(string OrderId, IMessageContext Context) : MessageContext(Context);

        [EventType("V1.OrderNeedsToBeCancelled")]
        public record OrderNeedsToBeCancelled(string OrderId, List<string> ProductIds, IMessageContext Context) : MessageContext(Context);
    }
}
