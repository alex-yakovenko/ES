using ES.Core;

namespace ES.Test.Aggregates.Inventory.Commands;

public class CancelProductReservationCommandHandler :
    EsCommandHandler<InventoryCommands.CancelProductReservationCommand, InventoryItem>
{
    public override Task Handle(InventoryCommands.CancelProductReservationCommand command, InventoryItem inventoryItem)
    {

        inventoryItem.PushNewEvent(new InventoryEvents.ProductReservationCanceled(
            inventoryItem.Id,
            command.OrderId
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        });

        return Task.CompletedTask;
    }
}
