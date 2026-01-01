using ES.Declarations;
using Eventuous;

namespace ES.Declarations.Orders
{
    public record OrderState : State<OrderState>
    {
        public string OrderId { get; set; } = "";
        public string TenantId { get; set; } = "";
        public string CustomerId { get; private set; } = "";
        public DateOnly Date { get; private set; }
        public List<OrderItem> Items { get; private set; } = [];
        public string Status { get; private set; } = "";
        public string? Note { get; private set; }

        public OrderState()
        {
            On<Events.OrderDrafted>((state, e) => state with
            {
                OrderId = e.OrderId,
                TenantId = e.TenantId,
                CustomerId = e.CustomerId,
                Date = e.Date,
                Items = [.. e.Items],
                Status = OrderStatus.Draft
            });

            On<Events.OrderCanceled>((state, e) => state with
            {
                Status = OrderStatus.Cancelled,
                Note = e.Reason
            });

            On<Events.OrderPlaced>((state, e) => state with
            {
                Status = OrderStatus.Placed
            });

            On<Events.ItemsAdjusted>((state, e) =>
            {
                foreach (var change in e.Changes)
                {
                    var item = state.Items.Find(x => x.ProductId == change.ProductId);

                    if (item == null)
                    {
                        item = new OrderItem()
                        {
                            ProductId = change.ProductId,
                            Quantity = change.AdjustQuantityBy
                        };

                        state.Items.Add(item);
                    }
                    else
                    {
                        item.Quantity += change.AdjustQuantityBy;
                        if (item.Quantity == 0)
                            state.Items.Remove(item);
                    }
                }

                return state;
            });

        }

        public record OrderItemChange(string ProductId, int AdjustQuantityBy);
    }

}