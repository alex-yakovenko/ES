using ES.Core;
using ES.Declarations;

namespace ES.Inventory.Commands;

public class CancelProductReservationCommandHandler :
    EsCommandHandler<InventoryItem.Commands.CancelProductReservationCommand, InventoryItem>
{
    public override Task Handle(InventoryItem.Commands.CancelProductReservationCommand command, InventoryItem inventoryItem)
    {

        inventoryItem.PushNewEvent(new InventoryItem.Events.ProductReservationCanceled(
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
