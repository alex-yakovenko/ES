using ES.Core;

namespace ES.Test.Aggregates.Orders;

public class OrderEvents
{
    public const string Stream = "Orders";

    public record OrderDrafted(
        string AggregateId,
        string CustomerId,
        DateOnly Date,
        List<OrderItem> Items
    ) : EsEvent(AggregateId, Stream);

    public record OrderCanceled(
        string AggregateId,
        string Reason
    ) : EsEvent(AggregateId, Stream);

    public record OrderPlaced(
        string AggregateId
    ) : EsEvent(AggregateId, Stream);

    public record ItemsAdjusted(
        string AggregateId,
        Order.OrderItemChange[] Changes
    ) : EsEvent(AggregateId, Stream);

    public record ItemsAdjustingFialeded(
        string AggregateId
    ) : EsEvent(AggregateId, Stream); 
}
