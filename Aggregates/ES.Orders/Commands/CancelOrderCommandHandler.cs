using ES.Core;
using ES.Declarations;

namespace ES.Orders.Commands;

public class CancelOrderCommandHandler :
    EsCommandHandler<Order.Commands.CancelOrder, Order>
{
    public override Task Handle(Order.Commands.CancelOrder command, Order aggregate)
    {
        aggregate.PushNewEvent(new Order.Events.OrderCanceled(
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
