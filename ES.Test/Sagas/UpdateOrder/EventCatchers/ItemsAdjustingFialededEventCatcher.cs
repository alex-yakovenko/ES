using ES.Core;
using ES.Test.Aggregates.Inventory;
using ES.Test.Aggregates.Orders;

namespace ES.Test.Sagas.UpdateOrder.EventCatchers;

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
