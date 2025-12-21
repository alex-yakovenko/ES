using ES.Core;
using ES.Test.Aggregates.Inventory;
using ES.Test.Aggregates.Orders;

namespace ES.Test.Sagas.UpdateOrder.EventCatchers;

public class ItemsAdjustedEventCatcher()
    : EsEventCatcher<UpdateOrderSaga, OrderEvents.ItemsAdjusted>(InventoryEvents.Stream)
{
    public override Task<bool> HandleEvent(UpdateOrderSaga saga, OrderEvents.ItemsAdjusted @event)
    {

        saga.PushNewEvent(new UpdateOrderSaga.Events.OrderUpdateStatus(saga.Id, true)
        {
            TenantId = @event.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        return Task.FromResult(true);
    }
}