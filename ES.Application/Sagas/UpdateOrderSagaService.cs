using ES.Declarations.UpdateOrderSaga;
using ES.Core;
using Eventuous;
using System.Linq;
using System.Collections.Generic;

namespace ES.Application.Sagas
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
                        cmd.TenantId, $"UpdateOrderSaga:{cmd.SagaId}")
                ]);

            On<Commands.UpdateStatus>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.OrderUpdateStatus(cmd.Success, cmd.TenantId, cmd.CorrelationId)
                ]);

            On<Commands.MarkProductReservedOrReleased>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    var eventsToReturn = new List<object>
                        { new Events.ProductReservedOrReleased(cmd.ProductId, cmd.TenantId, cmd.CorrelationId) };

                    var allReserved = state.Changes.All(x =>
                        x.ProductId == cmd.ProductId ? true : x.ReservedSuccessfuly
                    );

                    if (allReserved)
                    {
                        var changes = state.Changes
                            .Select(x => new OrderItemChange(x.ProductId, x.AdjustQuantityBy))
                            .ToArray();

                        eventsToReturn.Add(new Events.AllProductsReserved(state.OrderId, changes, cmd.TenantId,
                            cmd.CorrelationId));
                    }

                    return eventsToReturn;
                });

            On<Commands.MarkProductReservationFailed>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    var eventsToReturn = new List<object>
                        { new Events.ReservationFailed(cmd.ProductId, cmd.TenantId, cmd.CorrelationId) };

                    var productIdsToCancelReservations = state.Changes
                        .Where(s => s.ReservedSuccessfuly)
                        .Select(x => x.ProductId)
                        .ToArray();

                    eventsToReturn.Add(new Events.ReservationsCancellingNeeded(state.OrderId,
                        productIdsToCancelReservations, cmd.TenantId, cmd.CorrelationId));

                    return eventsToReturn;
                });
        }
    }
}
