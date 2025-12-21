using ES.Core;

namespace ES.Test.Aggregates.Orders.Commands;

public class CancelOrderCommandHandler :
    EsCommandHandler<OrderCommands.CancelOrder, Order>
{
    public override Task Handle(OrderCommands.CancelOrder command, Order aggregate)
    {
        aggregate.PushNewEvent(new OrderEvents.OrderCanceled(
            aggregate.Id,
            command.Reason
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        });

        return Task.CompletedTask;
    }
}
