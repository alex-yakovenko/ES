using ES.Core;

namespace ES.Test.Aggregates.Inventory;

public static class InventoryCommands
{
    public record CreateInventoryItemCommand(string AggregateId, string Name, int InitialQuantity) : EsCommand<InventoryItem>(AggregateId);

    public record ReserveProductCommand(string AggregateId, int Quantity, string OrderId) : EsCommand<InventoryItem>(AggregateId);

    public record CancelProductReservationCommand(string AggregateId, string OrderId) : EsCommand<InventoryItem>(AggregateId);

    public record ReleaseProductCommand(string AggregateId, int IncreaseQuantityBy, string OrderId) : EsCommand<InventoryItem>(AggregateId);
}
