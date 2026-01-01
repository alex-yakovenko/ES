using ES.Declarations.UpdateOrderSaga;
using Eventuous;

namespace ES.Test.Sagas
{
    public record UpdateOrderSagaId(string Id) : Id(Id);
    public class UpdateOrderSagaService : CommandService<UpdateOrderSagaAggregate, UpdateOrderSagaState, UpdateOrderSagaId>
    {
        public UpdateOrderSagaService(IEventReader? reader, IEventWriter? writer) : base(reader, writer)
        {
            On<Commands.Start>()
                .InState(ExpectedState.New)
                .GetId(x => new UpdateOrderSagaId(x.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.Start(cmd.SagaId, cmd.OrderId, cmd.Changes, cmd);
                });

            On<Commands.UpdateStatus>()
                .InState(ExpectedState.Existing)
                .GetId(x => new UpdateOrderSagaId(x.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.UpodateStatus(cmd.Success, cmd);
                });

            On<Commands.MarkProductReservedOrReleased>()
                .InState(ExpectedState.Existing)
                .GetId(x => new UpdateOrderSagaId(x.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.MarkProductReservedOrReleased(cmd.ProductId, cmd);
                });

            On<Commands.MarkProductReservationFailed>()
                .InState(ExpectedState.Existing)
                .GetId(x => new UpdateOrderSagaId(x.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.MarkProductReservationFailed(cmd.ProductId, cmd);
                });
        }
    }
}
