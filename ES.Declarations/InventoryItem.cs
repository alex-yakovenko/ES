using ES.Core;

namespace ES.Declarations;

public class InventoryItem : AggregateRoot
{
    public const string Stream = "Inventory";
    public string Name { get; private set; } = "";
    public int AvailableQuantity { get; private set; }

    public List<ProductReservations> Reservations { get; private set; } = [];

    public override string StreamType => Stream;

    public void Apply(Events.InventoryItemCreated e)
    {
        Id = e.AggregateId;
        Name = e.Name;
        AvailableQuantity = e.InitialQuantity;
        TenantId = e.TenantId;
    }

    public void Apply(Events.ProductReserved e)
    {
        AvailableQuantity -= e.Quantity;

        Reservations.Add(new ProductReservations
        {
            OrderId = e.OrderId,
            Quantity = e.Quantity,
            CorrelationId = e.CorrelationId
        });
    }

    public void Apply(Events.ProductReservationFailed e)
    {

    }

    public void Apply(Events.ProductReservationCanceled e) 
    { 
        var reservation = Reservations
            .First(x => x.CorrelationId == e.CorrelationId && x.OrderId == e.OrderId);

        AvailableQuantity += reservation.Quantity;

        Reservations.Remove(reservation);
    }

    public void Apply(Events.ProductReleased e) 
    { 
        Reservations.Add(new()
        {
            OrderId = e.OrderId,
            Quantity = -e.Quantity,
            CorrelationId = e.CorrelationId
        });

        AvailableQuantity += e.Quantity;
    }

    public static class Events
    {
        public record InventoryItemCreated(
            string AggregateId,
            string Name,
            int InitialQuantity
        ) : EsEvent(AggregateId, Stream);

        public record ProductReserved(
            string AggregateId,
            int Quantity,
            string OrderId
        ) : EsEvent(AggregateId, Stream);

        public record ProductReservationFailed(
            string AggregateId,
            int Quantity,
            string OrderId
        ) : EsEvent(AggregateId, Stream);

        public record ProductReservationCanceled(
            string AggregateId,
            string OrderId
        ) : EsEvent(AggregateId, Stream);

        public record ProductReleased(
            string AggregateId,
            int Quantity,
            string OrderId
        ) : EsEvent(AggregateId, Stream);
    }

    public static class Commands
    {
        public record CreateInventoryItemCommand(string AggregateId, string Name, int InitialQuantity) : EsCommand<InventoryItem>(AggregateId);

        public record ReserveProductCommand(string AggregateId, int Quantity, string OrderId) : EsCommand<InventoryItem>(AggregateId);

        public record CancelProductReservationCommand(string AggregateId, string OrderId) : EsCommand<InventoryItem>(AggregateId);

        public record ReleaseProductCommand(string AggregateId, int IncreaseQuantityBy, string OrderId) : EsCommand<InventoryItem>(AggregateId);
    }

}

public record ProductReservations
{
    public required string OrderId { get; set; }
    public int Quantity { get; set; }
    public required string CorrelationId { get; set; }
}
