using ES.Core;

namespace ES.Test.Aggregates.Inventory.Commands;

public class ReserveProductCommandHandler :
    EsCommandHandler<InventoryCommands.ReserveProductCommand, InventoryItem>
{
    public override Task Handle(InventoryCommands.ReserveProductCommand command, InventoryItem inventoryItem)
    {
        if (inventoryItem.AvailableQuantity - command.Quantity < 0)
        {
            inventoryItem.PushNewEvent(new InventoryEvents.ProductReservationFailed(
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
            inventoryItem.PushNewEvent(new InventoryEvents.ProductReserved(
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
