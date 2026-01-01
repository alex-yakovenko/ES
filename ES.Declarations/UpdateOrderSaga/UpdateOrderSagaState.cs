using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public record UpdateOrderSagaState : State<UpdateOrderSagaState>
    {
        public string TenantId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string? Result { get; set; }
        public bool? OrderUpdateSuccess { get; set; }
        public List<OrderChangeWithFlafs> Changes { get; set; } = [];

        public UpdateOrderSagaState() 
        {

            On<Events.Started>((state, evt) => state with
            {
                Changes = [..evt
                    .Changes
                    .Select(x => new OrderChangeWithFlafs(
                        x.ProductId,
                        x.AdjustQuantityBy
                    ))],
                OrderId = evt.OrderId,
                TenantId = evt.TenantId
            });

            On<Events.ProductReservedOrReleased>((state, evt) =>
            {
                Changes.First(x => x.ProductId == evt.ProductId).ReservedSuccessfuly = true;
                return state;
            });

            On<Events.ReservationFailed>((state, evt) =>
            {
                Changes.First(x => x.ProductId == evt.ProductId).FailedToReserve = true;
                return state;
            });

            On<Events.SetResult>((state, evt) => state with
            {
                Result = evt.Result
            });

            On<Events.OrderUpdateStatus>((state, evt) => state with
            {
                OrderUpdateSuccess = evt.Success
            });
    }
}

    public record OrderItemChange(string ProductId, int AdjustQuantityBy);

    public record OrderChangeWithFlafs(string ProductId, int AdjustQuantityBy)
    {
        public bool ReservedSuccessfuly { get; set; }
        public bool FailedToReserve { get; set; }
    };
}
