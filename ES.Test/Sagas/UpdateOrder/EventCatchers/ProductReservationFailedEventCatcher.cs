using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.UpdateOrder.EventCatchers;

public class ProductReservationFailedEventCatcher() : 
    EsEventCatcher<UpdateOrderSaga, InventoryEvents.ProductReservationFailed>(InventoryEvents.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, InventoryEvents.ProductReservationFailed @event)
    {
        saga.PushNewEvent(new UpdateOrderSaga.Events.RezervationFailed(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
