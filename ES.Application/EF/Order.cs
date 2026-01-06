namespace ES.Application.EF;

public record Order
{
    public string OrderId { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public DateOnly OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public string Status { get; set; } = "";
}