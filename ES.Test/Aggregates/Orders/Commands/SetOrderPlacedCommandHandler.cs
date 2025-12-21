using ES.Core;

namespace ES.Test.Aggregates.Orders.Commands;

public class SetOrderPlacedCommandHandler :
    EsCommandHandler<OrderCommands.SetOrderPlaced, Order>
{
    public override Task Handle(OrderCommands.SetOrderPlaced command, Order aggregate)
    {
        aggregate.PushNewEvent(new OrderEvents.OrderPlaced(
            command.AggregateId
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        });

        return Task.CompletedTask;
    }
}