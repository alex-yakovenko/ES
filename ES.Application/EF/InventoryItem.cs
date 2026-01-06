using System;
using System.Collections.Generic;
using System.Text;

namespace ES.Application.EF
{
    public record InventoryItem
    {
        public string ProductId { get; set; } = "";
        public int AvailableQuantity { get; set; }
        public List<ProductReservation> Reservations { get; set; } = [];
    }
}
