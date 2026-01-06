using ES.Declarations;
using Eventuous;

namespace ES.Declarations.Inventory
{
    public record InventoryState : State<InventoryState>
    {
        public string TenantId { get; private set; } = "";
        public string ProductId { get; private set; } = "";
        public int AvailableQuantity { get; private set; }

        public List<ProductReservations> Reservations { get; private set; } = [];

        public InventoryState()
        {
            On<Events.InventoryItemCreated>((state, evt) =>
                 state with
                 {

                     ProductId = evt.ProductId,
                     AvailableQuantity = evt.InitialQuantity,
                     TenantId = evt.TenantId
                 });


            On<Events.ProductReserved>((state, e) =>
            {
                state.AvailableQuantity -= e.Quantity;

                state.Reservations.Add(new ProductReservations
                {
                    OrderId = e.OrderId,
                    Quantity = e.Quantity,
                    CorrelationId = e.CorrelationId
                });

                return state;
            });

            On<Events.ProductReservationCanceled>((state, e) =>
            {
                var reservation = Reservations
                    .First(x => x.CorrelationId == e.CorrelationId && x.OrderId == e.OrderId);

                state.AvailableQuantity += reservation.Quantity;

                state.Reservations.Remove(reservation);

                return state;
            });

            On<Events.ProductReleased>((state, e) =>
            {
                state.Reservations.Add(new()
                {
                    OrderId = e.OrderId,
                    Quantity = -e.Quantity,
                    CorrelationId = e.CorrelationId!
                });

                state.AvailableQuantity += e.Quantity;
                return state;
            });
        }

    }

    public record ProductReservations
    {
        public required string OrderId { get; set; }
        public int Quantity { get; set; }
        public required string CorrelationId { get; set; }
    }
}
