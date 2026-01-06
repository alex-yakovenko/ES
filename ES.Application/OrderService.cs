using ES.Declarations.Orders;
using ES.Declarations;
using Eventuous;
using System.Linq;

namespace ES.Application
{
    public class OrderService : CommandService<OrderState>
    {
        public OrderService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.DraftOrder>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"Order-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderDrafted(cmd.OrderId, cmd.CustomerId, cmd.Date, cmd.Items, cmd.TenantId,
                        cmd.CorrelationId)
                ]);

            On<Commands.SetOrderPlaced>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Order-{cmd.OrderId}"))
                .Act((state, events, cmd) => events.OfType<Events.OrderPlaced>()
                    .Any(x => x.CorrelationId == cmd.CorrelationId) 
                        ?[]
                        :[
                            new Events.OrderPlaced(cmd.OrderId, cmd.TenantId, cmd.CorrelationId)
                        ]);

            On<Commands.CancelOrder>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Order-{cmd.OrderId}"))
                .Act((state, events, cmd) => events.OfType<Events.OrderCanceled>()
                    .Any(x => x.CorrelationId == cmd.CorrelationId)
                        ?[]
                        :[
                            new Events.OrderCanceled(cmd.OrderId, cmd.Reason, cmd.TenantId, cmd.CorrelationId)
                        ]);

            On<Commands.AdjustProducts>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"Order-{cmd.OrderId}"))
                .Act((state, events, cmd) =>
                {
                    if (events.OfType<Events.ItemsAdjusted>()
                        .Any(x => x.CorrelationId == cmd.CorrelationId))
                    {
                        // Idempotency check - adjustment for this correlation ID already exists
                        return [];
                    }

                    if (events.OfType<Events.ItemsAdjustingFialeded>()
    .Any(x => x.CorrelationId == cmd.CorrelationId))
                    {
                        // Idempotency check - adjustment for this correlation ID already exists
                        return [];
                    }

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
                        return [new Events.ItemsAdjustingFialeded(cmd.OrderId, cmd.TenantId, cmd.CorrelationId)];
                    }
                    else
                    {
                        return
                        [
                            new Events.ItemsAdjusted(cmd.OrderId,
                                cmd.Changes, cmd.TenantId, cmd.CorrelationId
                            )
                        ];
                    }
                });
        }
    }
}
