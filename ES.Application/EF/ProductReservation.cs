namespace ES.Application.EF
{
    public record ProductReservation
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string CorrelationId { get; set; } = "";
        public int Quantity { get; set; }
        public virtual InventoryItem? InventoryItem { get; set; } = null;
    }
}
