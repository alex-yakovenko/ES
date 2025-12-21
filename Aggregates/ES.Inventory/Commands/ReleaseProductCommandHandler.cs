using ES.Core;
using ES.Declarations;

namespace ES.Inventory.Commands;

public class ReleaseProductCommandHandler :
    EsCommandHandler<InventoryItem.Commands.ReleaseProductCommand, InventoryItem>
{
    public override Task Handle(InventoryItem.Commands.ReleaseProductCommand command, InventoryItem inventoryItem)
    {
        var reservedForOrder = inventoryItem
            .Reservations
            .Where(x => x.OrderId == command.OrderId)
            .Sum(x => x.Quantity);

        if (command.IncreaseQuantityBy > reservedForOrder)
        {
            inventoryItem.PushNewEvent(new InventoryItem.Events.ProductReservationFailed(
                inventoryItem.Id,
                command.IncreaseQuantityBy,
                command.OrderId
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });
        }
        else
        {
            inventoryItem.PushNewEvent(new InventoryItem.Events.ProductReleased(
                inventoryItem.Id,
                command.IncreaseQuantityBy,
                command.OrderId
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });
        }

        return Task.CompletedTask;
    }
}