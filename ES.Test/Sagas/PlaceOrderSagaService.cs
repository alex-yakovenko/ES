using ES.Declarations;
using ES.Declarations.PlaceOrderSaga;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Commands = ES.Declarations.PlaceOrderSaga.Commands;

namespace ES.Test.Sagas
{

    public record PlaceOrderSagaId(string Id): Id(Id);

    public class PlaceOrderSagaService : CommandService<PlaceOrderSagaAggregate, PlaceOrderSagaState, PlaceOrderSagaId>
    {
        public PlaceOrderSagaService(IEventReader? reader, IEventWriter? writer) : base(reader, writer)
        {
            On<Commands.Start>()
                .InState(ExpectedState.New)
                .GetId(cmd => new PlaceOrderSagaId(cmd.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.Start(cmd);
                });

            On<Commands.MarkProductReserved>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new PlaceOrderSagaId(cmd.SagaId))
                .Act((saga, cmd) => 
                { 
                    saga.ReserveProduct(cmd.ProductId, cmd); 
                });

            On<Commands.MarkProductReservationFailed>()
                .InState(ExpectedState.Existing)
                .GetId(cmd => new PlaceOrderSagaId(cmd.SagaId))
                .Act((saga, cmd) =>
                {
                    saga.CancelProductReservation(cmd.ProductId, cmd);
                });
        }
    }
}
