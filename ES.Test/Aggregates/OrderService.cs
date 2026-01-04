using ES.Declarations.Orders;
using ES.Declarations;
using Eventuous;
using System.Linq;

namespace ES.Test.Aggregates
{
    public class OrderService : CommandService<OrderState>
    {
        public OrderService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.DraftOrder>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"OrderAggregate-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderDrafted(cmd.OrderId, cmd.CustomerId, cmd.Date, cmd.Items, cmd)
                ]);

            On<Commands.SetOrderPlaced>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"OrderAggregate-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderPlaced(cmd)
                ]);

            On<Commands.CancelOrder>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"OrderAggregate-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderCanceled(cmd.Reason, cmd)
                ]);

            On<Commands.AdjustProducts>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"OrderAggregate-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                {
                    var items = state.Items.Select(x => x with { }).ToList();

                    foreach (var change in cmd.Changes)
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
                        return [new Events.ItemsAdjustingFialeded(cmd)];
                    }
                    else
                    {
                        return
                        [
                            new Events.ItemsAdjusted(
                                cmd.Changes, cmd
                            )
                        ];
                    }
                });
        }
    }
}
