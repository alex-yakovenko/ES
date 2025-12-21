using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.Steps
{
    public class PerformUpdateStep(ICommandQueue commandQueue) : SagaStep<UpdateOrderSaga>
    {
        public override async Task Execute(UpdateOrderSaga saga)
        {
            var changes = saga.Changes
                .Select(x => new Order.OrderItemChange(x.ProductId, x.AdjustQuantityBy))
                .ToArray();

            await commandQueue.SendCommand(new Order.Commands.AdjustProducts(saga.OrderId, changes)
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });
        }

        public override bool NeedsExecution(UpdateOrderSaga saga) =>
            saga.OrderUpdateSuccess == null &&
            saga.Changes.All(x => x.ReservedSuccessfuly) && !StepEverExecuted(saga);
    }
}
