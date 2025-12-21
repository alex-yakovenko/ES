using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.PaceOrder.EventCatchers;

public class ProductReservationFailedEventCatcher() : 
    EsEventCatcher<PlaceOrderSaga, InventoryItem.Events.ProductReservationFailed>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(PlaceOrderSaga saga, InventoryItem.Events.ProductReservationFailed @event)
    {
        saga.PushNewEvent(new PlaceOrderSaga.Events.ProductReservationFailed(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
