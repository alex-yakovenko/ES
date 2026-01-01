using ES.Declarations.Inventory;
using Eventuous;

namespace ES.Test.Aggregates
{
    public class InventoryService
        : CommandService<InventoryAggregate, InventoryState, InventoryId>
    {
        public InventoryService(IEventReader? reader, IEventWriter? writer, AggregateFactoryRegistry? factoryRegistry = null, StreamNameMap? streamNameMap = null, ITypeMapper? typeMap = null, AmendEvent? amendEvent = null) 
            : base(reader, writer, factoryRegistry, streamNameMap, typeMap, amendEvent)
        {
            On<Commands.CreateInventoryItem>()
                .InState(ExpectedState.New)
                .GetId(cmd => new InventoryId(cmd.ProductId))
                .Act((inventory, cmd) => inventory.CreateInventoryItem(cmd.ProductId, cmd.InitialQuantity, cmd));

            On<Commands.ReserveProduct>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new InventoryId(cmd.ProductId))
                .Act((inventory, cmd) => inventory.ReserveProduct(cmd.Quantity, cmd.OrderId, cmd));

            On<Commands.ReleaseProduct>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new InventoryId(cmd.ProductId))
                .Act((inventory, cmd) => inventory.ReleaseProduct(cmd.IncreaseQuantityBy, cmd.OrderId, cmd));

            On<Commands.CancelProductReservation>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new InventoryId(cmd.ProductId))
                .Act((inventory, cmd) => inventory.CancelProductReservation(cmd.OrderId, cmd));

        }
    }

    public record InventoryId(string Value) : Id(Value);
}
