using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.PaceOrder.EventCatchers;

public class ProductReservedEventCatcher() 
    : EsEventCatcher<PlaceOrderSaga, InventoryEvents.ProductReserved>(InventoryEvents.Stream)
{
    public override Task<bool> HandleEvent(PlaceOrderSaga saga, InventoryEvents.ProductReserved @event)
    {
        saga.PushNewEvent(new PlaceOrderSaga.Events.ProductReserved(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
