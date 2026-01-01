namespace ES.Declarations;

public record OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}
public record OrderItemChange(string ProductId, int AdjustQuantityBy);