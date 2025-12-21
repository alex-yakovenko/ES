namespace ES.Test.Aggregates.Orders;

public record OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}
