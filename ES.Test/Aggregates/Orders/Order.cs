using ES.Core;

namespace ES.Test.Aggregates.Orders;

public class Order : AggregateRoot
{
    public override string StreamType => OrderEvents.Stream;
    public string CustomerId { get; private set; }
    public DateOnly Date { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public string Status { get; private set; } = "";
    public string? Note { get; private set; }
    
    public void Apply(OrderEvents.OrderDrafted e)
    {
        Id = e.AggregateId;
        CustomerId = e.CustomerId;
        Date = e.Date;
        Items.AddRange(e.Items);
        Status = OrderStatus.Draft;
    }

    public void Apply(OrderEvents.OrderCanceled e)
    {
        Status = OrderStatus.Cancelled;
        Note = e.Reason;
    }

    public void Apply(OrderEvents.OrderPlaced e)
    {
        Status = OrderStatus.Placed;
    }

    public void Apply(OrderEvents.ItemsAdjusted e)
    {
        foreach (var change in e.Changes)
        {
            var item = Items.Find(x => x.ProductId == change.ProductId);

            if (item == null)
            {
                item = new OrderItem()
                {
                    ProductId = change.ProductId,
                    Quantity = change.AdjustQuantityBy
                };

                Items.Add(item);
            }
            else
            {
                item.Quantity += change.AdjustQuantityBy;
                if (item.Quantity == 0)
                    Items.Remove(item);
            }
        }
    }


    public record OrderItemChange(string ProductId, int AdjustQuantityBy);

}
