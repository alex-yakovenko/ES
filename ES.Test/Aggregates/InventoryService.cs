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

    public class InventoryService1
       : CommandService<InventoryState>
    {
        public InventoryService1(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Declarations.Inventory.Commands.CreateInventoryItem>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) => 
                {
                    return [new Declarations.Inventory.Events.InventoryItemCreated(
                        cmd.ProductId,
                        cmd.InitialQuantity,
                        cmd
                    )];
                });

            On<Declarations.Inventory.Commands.ReserveProduct>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) => 
                {
                    if (state.AvailableQuantity - cmd.Quantity < 0)
                    {
                        return [new Declarations.Inventory.Events.ProductReservationFailed(
                            cmd.ProductId,
                            cmd.Quantity,
                            cmd.OrderId, cmd)];
                    }
                    else
                    {
                        return [new Declarations.Inventory.Events.ProductReserved(
                            cmd.ProductId,
                            cmd.Quantity,
                            cmd.OrderId, cmd
                        )];
                    }
                });

            On<Declarations.Inventory.Commands.ReleaseProduct>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) => 
                {
                    var reservedForOrder = state
                        .Reservations
                        .Where(x => x.OrderId == cmd.OrderId)
                        .Sum(x => x.Quantity);

                    if (cmd.IncreaseQuantityBy > reservedForOrder)
                    {
                        return [new Events.ProductReservationFailed(
                            cmd.ProductId,
                            cmd.IncreaseQuantityBy,
                            cmd.OrderId, cmd
                        )];
                    }
                    else
                    {
                        return [new Events.ProductReleased(
                            cmd.ProductId,
                            cmd.IncreaseQuantityBy,
                            cmd.OrderId, cmd
                        )];
                    }
                });

            On<Commands.CancelProductReservation>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) =>
                {
                    return [new Events.ProductReservationCanceled(
                        cmd.OrderId, cmd
                    )];
                });
        }
    }
}
