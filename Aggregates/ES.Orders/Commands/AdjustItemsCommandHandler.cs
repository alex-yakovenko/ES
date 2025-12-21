using ES.Core;
using ES.Declarations;

namespace ES.Orders.Commands;

public class AdjustItemsCommandHandler :
    EsCommandHandler<Order.Commands.AdjustProducts, Order>
{
    public override Task Handle(Order.Commands.AdjustProducts command, Order order)
    {
        var items = order.Items.Select(x => x with { }).ToList();

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
            order.PushNewEvent(new Order.Events.ItemsAdjustingFialeded(
                order.Id
            )
            {
                CorrelationId = command.CorrelationId,
                TenantId = command.TenantId
            });

        }
        else 
        {
            order.PushNewEvent(new Order.Events.ItemsAdjusted(
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