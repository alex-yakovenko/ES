using ES.Core;
using ES.Test.Aggregates.Orders;

namespace ES.Test.Sagas.PaceOrder.Commands;

public class StartCommandHandler(ICommandQueue commandQueue) 
    : EsCommandHandler<PlaceOrderSaga.Commands.Start, PlaceOrderSaga>
{
    public override async Task Handle(PlaceOrderSaga.Commands.Start command, PlaceOrderSaga saga)
    {
        if (string.IsNullOrWhiteSpace(saga.Id))
        {
            saga.Id = Guid.NewGuid().ToString("N");
        }

        await commandQueue.SendCommand(new Order.Commands.DraftOrder(command.OrderId, command.CustomerId, command.Date, command.Items)
        {
            TenantId = command.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });

        saga.PushNewEvent(new PlaceOrderSaga.Events.Started(saga.Id, command.OrderId, command.Items)
        {
            TenantId = command.TenantId,
            CorrelationId = saga.GetCorrelationId()
        });
    }
}
