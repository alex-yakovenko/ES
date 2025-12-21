using ES.Core;

namespace ES.Test.Aggregates.Orders;

public class OrderCommands
{
    public record DraftOrder(
        string AggregateId,
        string CustomerId,
        DateOnly Date,
        List<OrderItem> Items
    ) : EsCommand<Order>(AggregateId);

    public record SetOrderPlaced(
        string AggregateId
    ) : EsCommand<Order>(AggregateId);


    public record CancelOrder(
        string AggregateId,
        string Reason
    ) : EsCommand<Order>(AggregateId);

    public record AdjustProducts(
        string AggregateId,
        Order.OrderItemChange[] Changes
    ) : EsCommand<Order>(AggregateId);

}