using ES.Declarations.Inventory;
using Eventuous;
using System.Linq;

namespace ES.Application.Inventory
{
    public class InventoryService : CommandService<InventoryState>
    {
        public InventoryService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.CreateInventoryItem>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.InventoryItemCreated(
                        cmd.ProductId,
                        cmd.InitialQuantity,
                        cmd.TenantId,
                        cmd.CorrelationId
                    )
                ]);

            On<Commands.ReserveProduct>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) =>
                {
                    if (state.AvailableQuantity - cmd.Quantity < 0)
                    {
                        return
                        [
                            new Events.ProductReservationFailed(
                                cmd.ProductId,
                                cmd.Quantity,
                                cmd.OrderId, cmd.TenantId, cmd.CorrelationId)
                        ];
                    }
                    else
                    {
                        return
                        [
                            new Events.ProductReserved(
                                cmd.ProductId,
                                cmd.Quantity,
                                cmd.OrderId, cmd.TenantId, cmd.CorrelationId
                            )
                        ];
                    }
                });

            On<Commands.ReleaseProduct>()
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
                        return
                        [
                            new Events.ProductReservationFailed(
                                cmd.ProductId,
                                cmd.IncreaseQuantityBy,
                                cmd.OrderId, cmd.TenantId, cmd.CorrelationId
                            )
                        ];
                    }
                    else
                    {
                        return
                        [
                            new Events.ProductReleased(
                                cmd.ProductId,
                                cmd.IncreaseQuantityBy,
                                cmd.OrderId, cmd.TenantId, cmd.CorrelationId
                            )
                        ];
                    }
                });

            On<Commands.CancelProductReservation>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Products-{cmd.ProductId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.ProductReservationCanceled(
                        cmd.OrderId, cmd.TenantId, cmd.CorrelationId
                    )
                ]);
        }
    }
}
