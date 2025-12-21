using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.EventCatchers;

public class ProductReservationFailedEventCatcher() : 
    EsEventCatcher<UpdateOrderSaga, InventoryItem.Events.ProductReservationFailed>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, InventoryItem.Events.ProductReservationFailed @event)
    {
        saga.PushNewEvent(new UpdateOrderSaga.Events.RezervationFailed(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
