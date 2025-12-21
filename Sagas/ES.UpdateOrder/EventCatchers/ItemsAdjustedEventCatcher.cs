using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.EventCatchers;

public class ItemsAdjustedEventCatcher()
    : EsEventCatcher<UpdateOrderSaga, Order.Events.ItemsAdjusted>(InventoryItem.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, Order.Events.ItemsAdjusted @event)
    {

        saga.PushNewEvent(new UpdateOrderSaga.Events.OrderUpdateStatus(saga.Id, true)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}