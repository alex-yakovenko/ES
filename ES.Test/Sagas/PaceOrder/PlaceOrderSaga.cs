using ES.Core;
using ES.Test.Aggregates.Orders;

namespace ES.Test.Sagas.PaceOrder;

public class PlaceOrderSaga : Saga
{
    public const string Stream = "PlaceOrderSaga";
    public string OrderId { get; private set; }
    public List<ItemToReserve> ItemsToReserve { get; } = [];
    public override string StreamType => Stream;

    public void Apply(Events.Started evt)
    {
        Id = evt.AggregateId;
        OrderId = evt.OrderId;
        TenantId = evt.TenantId;
        ItemsToReserve.AddRange(evt.Items.Select(x => new ItemToReserve
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            ReservationRequested = false
        }));

    }

    public void Apply(Events.RezervationSent evt)
    {
        var item = ItemsToReserve.First(x => x.ProductId == evt.ProductId);
        item.ReservationRequested = true;
    }

    public void Apply(Events.ProductReserved evt)
    {
        var item = ItemsToReserve.First(x => x.ProductId == evt.ProductId);
        item.ProductReserved = true;
    }

    public void Apply(Events.ProductReservationFailed evt)
    {
        var item = ItemsToReserve.First(x => x.ProductId == evt.ProductId);
        item.ReservationFailed = true;
    }
    
    public static class Events
    {
        public record Started(string AggregateId, string OrderId, List<OrderItem> Items) : EsEvent(AggregateId, Stream);
        public record RezervationSent(string AggregateId, string ProductId) : EsEvent(AggregateId, Stream);
        public record ProductReserved(string AggregateId, string ProductId) : EsEvent(AggregateId, Stream);
        public record ProductReservationFailed(string AggregateId, string ProductId) : EsEvent(AggregateId, Stream);

    }

    public static class Commands
    {
        public record Start(
            string AggregateId,
            string OrderId,
            string CustomerId,
            DateOnly Date,
            List<OrderItem> Items
        ) : EsCommand<PlaceOrderSaga>(AggregateId);
    }
}

public record ItemToReserve 
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public bool ReservationRequested { get; set; }
    public bool ProductReserved { get; set; }
    public bool ReservationFailed { get; set; }
}
