using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.EventCatchers;

public class ProductReleasedEventCatcher() 
    : EsEventCatcher<UpdateOrderSaga, InventoryItem.Events.ProductReleased>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, InventoryItem.Events.ProductReleased @event)
    {
        saga.PushNewEvent(new UpdateOrderSaga.Events.ProductReservedOrReleased(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}