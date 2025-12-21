using ES.Core;

namespace ES.Test.Aggregates.Inventory;

public class InventoryItem : AggregateRoot
{
    public string Name { get; private set; } = "";
    public int AvailableQuantity { get; private set; }

    public List<ProductReservations> Reservations { get; private set; } = [];

    public override string StreamType => InventoryEvents.Stream;

    public void Apply(InventoryEvents.InventoryItemCreated e)
    {
        Id = e.AggregateId;
        Name = e.Name;
        AvailableQuantity = e.InitialQuantity;
        TenantId = e.TenantId;
    }

    public void Apply(InventoryEvents.ProductReserved e)
    {
        AvailableQuantity -= e.Quantity;

        Reservations.Add(new ProductReservations
        {
            OrderId = e.OrderId,
            Quantity = e.Quantity,
            CorrelationId = e.CorrelationId
        });
    }

    public void Apply(InventoryEvents.ProductReservationFailed e)
    {

    }

    public void Apply(InventoryEvents.ProductReservationCanceled e) 
    { 
        var reservation = Reservations
            .First(x => x.CorrelationId == e.CorrelationId && x.OrderId == e.OrderId);

        AvailableQuantity += reservation.Quantity;

        Reservations.Remove(reservation);
    }

}

public record ProductReservations
{
    public string OrderId { get; set; }
    public int Quantity { get; set; }
    public string CorrelationId { get; set; }
}
