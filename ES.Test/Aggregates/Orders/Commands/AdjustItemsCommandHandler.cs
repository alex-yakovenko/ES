using ES.Core;

namespace ES.Test.Aggregates.Orders.Commands;

public class AdjustItemsCommandHandler :
    EsCommandHandler<OrderCommands.AdjustProducts, Order>
{
    public override Task Handle(OrderCommands.AdjustProducts command, Order order)
    {
        var items = order.Items;

        foreach (var change in command.Changes)
        {
            var item = items.Find(x => x.ProductId == change.ProductId);

            if (item == null)
            {
                item = new OrderItem()
                {
                    ProductId = change.ProductId,
                    Quantity = change.AdjustQuantityBy
                };
                items.Add(item);
            }
            else
            {
                item.Quantity += change.AdjustQuantityBy;
            }
        }

        if (items.Any(x => x.Quantity < 0))
        {
            order.PushNewEvent(new OrderEvents.ItemsAdjustingFialeded(
                order.Id
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });

        }
        else 
        {
            order.PushNewEvent(new OrderEvents.ItemsAdjusted(
                order.Id,
                command.Changes
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });
        }

        return Task.CompletedTask;
    }
}