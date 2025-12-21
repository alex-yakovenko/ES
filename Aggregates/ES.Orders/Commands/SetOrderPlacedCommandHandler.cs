using ES.Core;
using ES.Declarations;

namespace ES.Orders.Commands;

public class SetOrderPlacedCommandHandler :
    EsCommandHandler<Order.Commands.SetOrderPlaced, Order>
{
    public override Task Handle(Order.Commands.SetOrderPlaced command, Order aggregate)
    {
        aggregate.PushNewEvent(new Order.Events.OrderPlaced(
            command.AggregateId
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        });

        return Task.CompletedTask;
    }
}