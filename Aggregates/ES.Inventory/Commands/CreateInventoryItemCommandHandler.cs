using ES.Core;
using ES.Declarations;

namespace ES.Inventory.Commands;

public class CreateInventoryItemCommandHandler : EsCommandHandler<InventoryItem.Commands.CreateInventoryItemCommand, InventoryItem>
{
    public override Task Handle(InventoryItem.Commands.CreateInventoryItemCommand command, InventoryItem aggregate)
    {
        var @event = new InventoryItem.Events.InventoryItemCreated(
            command.AggregateId,
            command.Name,
            command.InitialQuantity
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        };

        aggregate.PushNewEvent(@event);

        return Task.CompletedTask;
    }
}
