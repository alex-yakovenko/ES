using ES.Core;
using Eventuous;

namespace ES.Declarations.PlaceOrderSaga
{
    public class PlaceOrderSagaAggregate : Aggregate<PlaceOrderSagaState>
    {
        public const string Name = "PlaceOrderSaga";
        public string SagaName => Name;

        public void Start(Commands.Start cmd)
        {
            Apply(new Events.Started(cmd.SagaId, cmd.OrderId, cmd.CustomerId, 
                cmd.Date, cmd.Items, new MessageContext { TenantId = cmd.TenantId, CorrelationId = $"{Name}:{cmd.SagaId}"}));
        }

        public void ReserveProduct(string productId, IMessageContext context)
        {
            Apply(new Events.ProductReserved(productId, context));

            if (State.ItemsToReserve.All(x => x.ProductReserved && !x.ReservationFailed) 
                && !State.OrderCanBePlaced)

            Apply(new Events.OrderCanBePlaced(State.OrderId, context));
        }

        public void CancelProductReservation(string productId, IMessageContext context)
        {
            Apply(new Events.ProductReservationFailed(productId, context));

            if (!State.OrderNeedsToBeCancelled) 
            {
                var reservationsToCancel = State
                    .ItemsToReserve
                    .Where(x => !x.ReservationFailed)
                    .Select(x => x.ProductId)
                    .ToList()!;

                Apply(new Events.OrderNeedsToBeCancelled(State.OrderId, reservationsToCancel, context));
            }
        }
    }
}
