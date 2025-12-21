using ES.Core;

namespace ES.Test.Aggregates.Orders.Commands;

public class DraftOrderCommandHandler :
    EsCommandHandler<OrderCommands.DraftOrder, Order>
{
    public override Task Handle(OrderCommands.DraftOrder command, Order aggregate)
    {
        aggregate.PushNewEvent(new OrderEvents.OrderDrafted(
            command.AggregateId,
            command.CustomerId,
            command.Date,
            command.Items
        )
        {
            CorrelationId = command.CorrelationId,
            TenantId = command.TenantId
        });

        return Task.CompletedTask;
    }
}