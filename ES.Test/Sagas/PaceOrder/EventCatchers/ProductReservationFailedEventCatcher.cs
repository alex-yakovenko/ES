using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.PaceOrder.EventCatchers;

public class ProductReservationFailedEventCatcher() : 
    EsEventCatcher<PlaceOrderSaga, InventoryEvents.ProductReservationFailed>(InventoryEvents.Stream)
{
    public override Task<bool> HandleEvent(PlaceOrderSaga saga, InventoryEvents.ProductReservationFailed @event)
    {
        saga.PushNewEvent(new PlaceOrderSaga.Events.ProductReservationFailed(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
