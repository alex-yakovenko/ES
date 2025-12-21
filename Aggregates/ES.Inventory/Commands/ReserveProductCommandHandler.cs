using ES.Core;
using ES.Declarations;

namespace ES.Inventory.Commands;

public class ReserveProductCommandHandler :
    EsCommandHandler<InventoryItem.Commands.ReserveProductCommand, InventoryItem>
{
    public override Task Handle(InventoryItem.Commands.ReserveProductCommand command, InventoryItem inventoryItem)
    {
        if (inventoryItem.AvailableQuantity - command.Quantity < 0)
        {
            inventoryItem.PushNewEvent(new InventoryItem.Events.ProductReservationFailed(
                inventoryItem.Id,
                command.Quantity,
                command.OrderId
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });
        }
        else
        {
            inventoryItem.PushNewEvent(new InventoryItem.Events.ProductReserved(
                inventoryItem.Id,
                command.Quantity,
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
