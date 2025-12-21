using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.EventCatchers;

public class ItemsAdjustingFialededEventCatcher() 
    : EsEventCatcher<UpdateOrderSaga, Order.Events.ItemsAdjustingFialeded>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, Order.Events.ItemsAdjustingFialeded @event)
    {

        saga.PushNewEvent(new UpdateOrderSaga.Events.OrderUpdateStatus(saga.Id, false)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}
