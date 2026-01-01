using ES.Declarations;
using Eventuous;
using System;
using System.Collections.Generic;
using System.Text;

namespace ES.Declarations.Orders
{
    public class OrderAggregate : Aggregate<OrderState>
    {
        public void DraftOrder(Commands.DraftOrder cmd)
        {
            Apply(new Events.OrderDrafted(cmd.OrderId, cmd.CustomerId, cmd.Date, cmd.Items, cmd));
        }

        public void SetOrderPlaced(Commands.SetOrderPlaced cmd)
        {
            Apply(new Events.OrderPlaced(cmd));
        }

        public void CancelOrder(Commands.CancelOrder cmd)
        {
            Apply(new Events.OrderCanceled(cmd.Reason, cmd));
        }

        public void AdjustProducts(Commands.AdjustProducts command)
        {
            var items = State.Items.Select(x => x with { }).ToList();

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
                Apply(new Events.ItemsAdjustingFialeded(command));

            }
            else
            {
                Apply(new Events.ItemsAdjusted(
                    command.Changes, command
                ));
            }
        }
    }

    public record OrderId(string id): Id(id);

}