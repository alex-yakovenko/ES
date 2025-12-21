using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.UpdateOrder.EventCatchers;

public class ProductReservedEventCatcher() 
    : EsEventCatcher<UpdateOrderSaga, InventoryItem.Events.ProductReserved>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, InventoryItem.Events.ProductReserved @event)
    {
        saga.PushNewEvent(new UpdateOrderSaga.Events.ProductReserved(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
