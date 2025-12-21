using ES.Core;
using ES.Test.Aggregates.Orders;

namespace ES.Test.Sagas.PaceOrder.Steps;

public class SetOrderPlacedStep(ICommandQueue commandQueue) : SagaStep<PlaceOrderSaga>
{
    public override bool NeedsExecution(PlaceOrderSaga saga) => 
        saga.ItemsToReserve.All(x => x.ProductReserved && !x.ReservationFailed) &&
        !StepEverExecuted(saga);

    public override async Task Execute(PlaceOrderSaga saga)
    {
        await commandQueue.SendCommand(new OrderCommands.SetOrderPlaced(saga.OrderId)
        {
            TenantId = saga.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });
    }
}
