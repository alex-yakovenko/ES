using ES.Core;
using ES.Declarations;

namespace ES.PlaceOrder.Steps;

public class CancelOrderStep(ICommandQueue commandQueue) : SagaStep<PlaceOrderSaga>
{
    public override bool NeedsExecution(PlaceOrderSaga saga) => 
        saga.ItemsToReserve.Any(x => x.ReservationFailed) && !StepEverExecuted(saga);

    public override async Task Execute(PlaceOrderSaga saga)
    {
        foreach (var item in saga.ItemsToReserve.Where(x => x.ProductReserved))
        {
            await commandQueue.SendCommand(new InventoryItem.Commands.CancelProductReservationCommand(item.ProductId, saga.OrderId)
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });
        }

        var insufficientProducts = string.Join(", ", saga.ItemsToReserve.Where(x => x.ReservationFailed).Select(x => x.ProductId));

        await commandQueue.SendCommand(new Order.Commands.CancelOrder(saga.OrderId, $"Not enough products to reserve: {insufficientProducts}.")
        {
            TenantId = saga.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });
    }
}
