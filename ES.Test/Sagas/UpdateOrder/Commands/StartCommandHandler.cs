using ES.Core;
using ES.Test.Aggregates.Inventory;

namespace ES.Test.Sagas.UpdateOrder.Commands
{
    public class StartCommandHandler(ICommandQueue commandQueue) 
        : EsCommandHandler<UpdateOrderSaga.Commands.Start, UpdateOrderSaga>
    {
        public override async Task Handle(UpdateOrderSaga.Commands.Start command, UpdateOrderSaga saga)
        {
            var reservationsSentFor = new List<string>();

            foreach (var item in command.Changes)
                if (item.AdjustQuantityBy > 0)
                {
                    await commandQueue.SendCommand(new InventoryCommands.ReserveProductCommand(item.ProductId, item.AdjustQuantityBy, command.OrderId)
                    {
                        TenantId = command.TenantId,
                        CorrelationId = saga.GetCorrelationId()
                    });

                    reservationsSentFor.Add(item.ProductId);
                }
                else
                {
                    await commandQueue.SendCommand(new InventoryCommands.ReleaseProductCommand(item.ProductId, -item.AdjustQuantityBy, command.OrderId)
                    {
                        TenantId = command.TenantId,
                        CorrelationId = saga.GetCorrelationId()
                    });
                }

            saga.PushNewEvent(new UpdateOrderSaga.Events.Started(command.AggregateId, command.Changes, command.OrderId, reservationsSentFor)
            {
                CorrelationId = saga.GetCorrelationId(),
                TenantId = command.TenantId
            });
        }
    }
}
