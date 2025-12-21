using ES.Core;

namespace ES.Declarations;

public class Order : AggregateRoot
{
    public const string Stream = "Orders";
    public override string StreamType => Stream;
    public string CustomerId { get; private set; } = "";
    public DateOnly Date { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public string Status { get; private set; } = "";
    public string? Note { get; private set; }
    
    public void Apply(Events.OrderDrafted e)
    {
        Id = e.AggregateId;
        CustomerId = e.CustomerId;
        Date = e.Date;
        Items.AddRange(e.Items);
        Status = OrderStatus.Draft;
    }

    public void Apply(Events.OrderCanceled e)
    {
        Status = OrderStatus.Cancelled;
        Note = e.Reason;
    }

    public void Apply(Events.OrderPlaced e)
    {
        Status = OrderStatus.Placed;
    }

    public void Apply(Events.ItemsAdjusted e)
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

    public static class Events
    {
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

    public static class Commands
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
}
