using ES.Declarations;
using Eventuous;

namespace ES.Declarations.PlaceOrderSaga
{
    public record PlaceOrderSagaState: State<PlaceOrderSagaState>
    {

        public string OrderId { get; private set; } = "";
        public List<ItemToReserve> ItemsToReserve { get; private set; } = [];

        public bool OrderCanBePlaced { get; set; }
        public bool OrderNeedsToBeCancelled { get; set; }

        public PlaceOrderSagaState() 
        {
            On<Events.Started>((state, evt) => state with 
            { 
                OrderId = evt.OrderId,
                ItemsToReserve = [.. evt.Items.Select(x => new ItemToReserve
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ReservationRequested = false
                })]
            });

            On<Events.RezervationSent>((state, evt) =>
            {
                var item = state
                    .ItemsToReserve
                    .First(x => x.ProductId == evt.ProductId);

                item.ReservationRequested = true;
                return state;
            });

            On<Events.ProductReserved>((state, evt) =>
            {
                var item = state
                    .ItemsToReserve
                    .First(x => x.ProductId == evt.ProductId);

                item.ProductReserved = true;
                return state;
            });

            On<Events.ProductReservationFailed>((state, evt) =>
            {
                var item = state
                    .ItemsToReserve
                    .First(x => x.ProductId == evt.ProductId);

                item.ReservationFailed = true;
                return state;
            });

            On<Events.OrderCanBePlaced>((state, evt) =>
            {
                return state with { OrderCanBePlaced = true };
            });
        }
    }

    public record ItemToReserve
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public bool ReservationRequested { get; set; }
        public bool ProductReserved { get; set; } 
        public bool ReservationFailed { get; set; }
    }
}
