using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.UpdateOrder.Steps
{
    public class RollbackUpdateStep(ICommandQueue commandQueue) : SagaStep<UpdateOrderSaga>
    {
        public override async Task Execute(UpdateOrderSaga saga)
        {
            foreach (var item in saga.Changes.Where(x => x.WaitsForReservation && !x.FailedToReserve))
                await commandQueue.SendCommand(new InventoryCommands.CancelProductReservationCommand(item.ProductId, saga.OrderId)
                {
                    TenantId = saga.TenantId,
                    CorrelationId = saga.GetCorrelationId()
                });

            saga.PushNewEvent(new UpdateOrderSaga.Events.SetResult(saga.Id, "Failed")
            {
                TenantId = saga.TenantId,
                CorrelationId = saga.GetCorrelationId()
            });
        }

        public override bool NeedsExecution(UpdateOrderSaga saga) => 
            (saga.OrderUpdateSuccess == false || saga.Changes.Any(x => x.FailedToReserve)) && !StepEverExecuted(saga);
    }

}
