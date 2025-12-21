using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.PaceOrder.Steps;

public class ReserveProductsStep(ICommandQueue commandQueue) : SagaStep<PlaceOrderSaga>
{
    public override bool NeedsExecution(PlaceOrderSaga saga)
    {
        return saga.ItemsToReserve.Any(x => !x.ReservationRequested);
    }

    public override async Task Execute(PlaceOrderSaga saga)
    {
        foreach (var item in saga.ItemsToReserve.Where(x => !x.ReservationRequested))
        {
            await commandQueue.SendCommand(new InventoryCommands.ReserveProductCommand(item.ProductId, item.Quantity, saga.OrderId)
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });

            saga.PushNewEvent(new PlaceOrderSaga.Events.RezervationSent(saga.Id, item.ProductId)
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });
        }
    }
}
