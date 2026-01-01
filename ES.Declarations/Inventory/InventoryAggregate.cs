using ES.Core;
using Eventuous;

namespace ES.Declarations.Inventory
{
    public class InventoryAggregate : Aggregate<InventoryState> 
    { 
        public void CreateInventoryItem(string Code, int InitialQuantity, IMessageContext context)
        {
            EnsureDoesntExist();

            Apply(new Events.InventoryItemCreated(
                Code,
                InitialQuantity,
                context
            ));
        }

        public void ReserveProduct(int quantity, string orderId, IMessageContext context)
        {
            EnsureExists();

            if (State.AvailableQuantity - quantity < 0)
            {
                Apply(new Events.ProductReservationFailed(
                    State.ProductId,
                    quantity,
                    orderId, context));
            }
            else
            {
                Apply(new Events.ProductReserved(
                    State.ProductId,
                    quantity,
                    orderId, context
                ));
            }
        }

        public void ReleaseProduct(int increaseQuantityBy, string orderId, IMessageContext context)
        {
            EnsureExists();

            var reservedForOrder = State
            .Reservations
            .Where(x => x.OrderId == orderId)
            .Sum(x => x.Quantity);

            if (increaseQuantityBy > reservedForOrder)
            {
                Apply(new Events.ProductReservationFailed(
                    State.ProductId,
                    increaseQuantityBy,
                    orderId, context
                ));
            }
            else
            {
                Apply(new Events.ProductReleased(
                    State.ProductId,
                    increaseQuantityBy,
                    orderId, context
                ));
            }
        }

        public void CancelProductReservation(string orderId, IMessageContext context)
        {
            EnsureExists();

            Apply(new Events.ProductReservationCanceled(
                orderId, context
            ));
        }
    }

}
