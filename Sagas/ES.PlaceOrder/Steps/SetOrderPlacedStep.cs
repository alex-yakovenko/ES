using ES.Core;
using ES.Declarations;

namespace ES.PlaceOrder.Steps;

public class SetOrderPlacedStep(ICommandQueue commandQueue) : SagaStep<PlaceOrderSaga>
{
    public override bool NeedsExecution(PlaceOrderSaga saga) => 
        saga.ItemsToReserve.All(x => x.ProductReserved && !x.ReservationFailed) &&
        !StepEverExecuted(saga);

    public override async Task Execute(PlaceOrderSaga saga)
    {
        await commandQueue.SendCommand(new Order.Commands.SetOrderPlaced(saga.OrderId)
        {
            TenantId = saga.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });
    }
}
