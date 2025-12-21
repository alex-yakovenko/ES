using ES.Core;
using ES.Declarations;

namespace ES.PlaceOrder.EventCatchers;

public class ProductReservedEventCatcher() 
    : EsEventCatcher<PlaceOrderSaga, InventoryItem.Events.ProductReserved>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(PlaceOrderSaga saga, InventoryItem.Events.ProductReserved @event)
    {
        saga.PushNewEvent(new PlaceOrderSaga.Events.ProductReserved(saga.Id, @event.AggregateId)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
