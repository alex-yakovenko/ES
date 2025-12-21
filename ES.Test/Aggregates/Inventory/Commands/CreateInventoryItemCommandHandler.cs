using ES.Core;

namespace ES.Test.Aggregates.Inventory.Commands;

public class CreateInventoryItemCommandHandler : EsCommandHandler<InventoryCommands.CreateInventoryItemCommand, InventoryItem>
{
    public override Task Handle(InventoryCommands.CreateInventoryItemCommand command, InventoryItem aggregate)
    {
        var @event = new InventoryEvents.InventoryItemCreated(
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
