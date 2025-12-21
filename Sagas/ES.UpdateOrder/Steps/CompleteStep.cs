using ES.Core;
using ES.Declarations;

namespace ES.UpdateOrder.Steps
{
    public class CompleteStep(ICommandQueue commandQueue) : SagaStep<UpdateOrderSaga>
    {
        public override async Task Execute(UpdateOrderSaga saga)
        {
            saga.PushNewEvent(new UpdateOrderSaga.Events.SetResult(saga.Id, "Done")
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });
        }

        public override bool NeedsExecution(UpdateOrderSaga saga) =>
            (saga.OrderUpdateSuccess == true && saga.Changes.All(x => x.ReservedSuccessfuly)) && !StepEverExecuted(saga);
    }

}
