using ES.Core;

namespace ES.Test.Aggregates.Inventory;

public class InventoryEvents
{
    public const string Stream = "Inventory";

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


}
