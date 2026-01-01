using ES.Declarations.Orders;
using Eventuous;

namespace ES.Test.Aggregates
{
    public class OrderService : CommandService<OrderAggregate, OrderState, OrderId>
    {
        public OrderService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.DraftOrder>()
                .InState(ExpectedState.New)
                .GetId(cmd => new OrderId(cmd.OrderId))
                .Act((order, cmd) => order.DraftOrder(cmd));

            On<Commands.SetOrderPlaced>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new OrderId(cmd.OrderId))
                .Act((order, cmd) => order.SetOrderPlaced(cmd));

            On<Commands.CancelOrder>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new OrderId(cmd.OrderId))
                .Act((order, cmd) => order.CancelOrder(cmd));

            On<Commands.AdjustProducts>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new OrderId(cmd.OrderId))
                .Act((order, cmd) => order.AdjustProducts(cmd));
        }
    }

}