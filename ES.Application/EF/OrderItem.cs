namespace ES.Application.EF;

public record OrderItem
{
  public int Id { get; set; }
  public string OrderId { get; set; } = "";
  public string ProductId { get; set; } = "";
  public int Quantity { get; set; }
  public Order? Order { get; set; } = null;
}