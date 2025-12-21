using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.EventCatchers;

public class ProductReservedEventCatcher() 
    : EsEventCatcher<UpdateOrderSaga, InventoryItem.Events.ProductReserved>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, InventoryItem.Events.ProductReserved @event)
    {
        saga.PushNewEvent(new UpdateOrderSaga.Events.ProductReservedOrReleased(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
