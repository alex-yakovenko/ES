using ES.Core;

namespace ES.Test.Aggregates.Orders.Commands;

public class DraftOrderCommandHandler :
    EsCommandHandler<Order.Commands.DraftOrder, Order>
{
    public override Task Handle(Order.Commands.DraftOrder command, Order aggregate)
    {
        aggregate.PushNewEvent(new Order.Events.OrderDrafted(
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