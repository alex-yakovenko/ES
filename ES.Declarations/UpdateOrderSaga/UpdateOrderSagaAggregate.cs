using ES.Core;
using ES.Declarations.Inventory;
using Eventuous;

namespace ES.Declarations.UpdateOrderSaga
{
    public class UpdateOrderSagaAggregate : Aggregate<UpdateOrderSagaState>
    {
        public const string Name = "UpdateOrderSaga";
        public string SagaName => Name;

        public void MarkProductReservationFailed(string productId, IMessageContext context)
        {
            Apply(new Events.ReservationFailed(productId, context));

            var productIdsToCancelReservations = State.Changes
                .Where(s => s.ReservedSuccessfuly)
                .Select(x => x.ProductId)
                .ToArray();

            Apply(new Events.ReservationsCancellingNeeded(State.OrderId, productIdsToCancelReservations, context));
        }

        public void MarkProductReservedOrReleased(string productId, IMessageContext context)
        {
            Apply(new Events.ProductReservedOrReleased(productId, context));

            if (State.Changes.All(x => x.ReservedSuccessfuly))
            {
                var changes = State.Changes
                    .Select(x => new OrderItemChange(x.ProductId, x.AdjustQuantityBy))
                    .ToArray();

                Apply(new Events.AllProductsReserved(State.OrderId, changes, context));
            }
        }

        public void Start(string sagaId, string orderId, List<OrderItemChange> changes, IMessageContext context)
        {
            Apply(new Events.Started(sagaId, changes, orderId, 
                new MessageContext(context.TenantId, $"{Name}:{sagaId}")));
        }

        public void UpodateStatus(bool success, IMessageContext context)
        {
            Apply(new Events.OrderUpdateStatus(success, context));
        }
    }
}
