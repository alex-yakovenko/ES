using ES.Declarations.UpdateOrderSaga;
using ES.Core;
using Eventuous;
using System.Linq;
using System.Collections.Generic;

namespace ES.Test.Sagas
{
    public class UpdateOrderSagaService : CommandService<UpdateOrderSagaState>
    {
        public UpdateOrderSagaService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.Start>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.Started(cmd.SagaId, cmd.Changes, cmd.OrderId,
                        new MessageContext(cmd.TenantId, $"UpdateOrderSaga:{cmd.SagaId}"))
                ]);

            On<Commands.UpdateStatus>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderUpdateStatus(cmd.Success, cmd)
                ]);

            On<Commands.MarkProductReservedOrReleased>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    var eventsToReturn = new List<object> { new Events.ProductReservedOrReleased(cmd.ProductId, cmd) };

                    var allReserved = state.Changes.All(x =>
                        x.ProductId == cmd.ProductId ? true : x.ReservedSuccessfuly
                    );

                    if (allReserved)
                    {
                        var changes = state.Changes
                            .Select(x => new OrderItemChange(x.ProductId, x.AdjustQuantityBy))
                            .ToArray();

                        eventsToReturn.Add(new Events.AllProductsReserved(state.OrderId, changes, cmd));
                    }

                    return eventsToReturn;
                });

            On<Commands.MarkProductReservationFailed>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    var eventsToReturn = new List<object> { new Events.ReservationFailed(cmd.ProductId, cmd) };

                    var productIdsToCancelReservations = state.Changes
                        .Where(s => s.ReservedSuccessfuly)
                        .Select(x => x.ProductId)
                        .ToArray();

                    eventsToReturn.Add(new Events.ReservationsCancellingNeeded(state.OrderId,
                        productIdsToCancelReservations, cmd));

                    return eventsToReturn;
                });
        }
    }
}
