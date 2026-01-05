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
            On<Commands.StartUpdate>()
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
                .Act((state, events, cmd) => events.OfType<Events.OrderUpdateStatus>()
                    .Any(x => x.CorrelationId == cmd.CorrelationId)
                        ?[]
                        :[
                            new Events.OrderUpdateStatus(cmd.Success, cmd.TenantId, cmd.CorrelationId)
                        ]);

            On<Commands.MarkProductReservedOrReleased>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    if (events.OfType<Events.ProductReservedOrReleased>()
                        .Any(x => x.ProductId == cmd.ProductId && x.CorrelationId == cmd.CorrelationId))
                    {
                        // Idempotency check - reservation for this product already exists
                        return [];
                    }

                    var eventsToReturn = new List<object>
                        { new Events.ProductReservedOrReleased(cmd.ProductId, cmd.TenantId, cmd.CorrelationId) };

                    var allReserved = state.Changes.All(x =>
                        x.ProductId == cmd.ProductId ? true : x.ReservedSuccessfuly
                    );

                    if (allReserved && !events.OfType<Events.AllProductsReserved>().Any())
                    {
                        var changes = state.Changes
                            .Select(x => new OrderItemChangeInfo(x.ProductId, x.AdjustQuantityBy))
                            .ToArray();

                        eventsToReturn.Add(new Events.AllProductsReserved(state.OrderId, changes, cmd.TenantId,
                            cmd.CorrelationId));
                    }

                    return eventsToReturn;
                });

            On<Commands.MarkProductReservationFailed1>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"UpdateOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    var eventsToReturn = new List<object>();

                    if (!events.OfType<Events.ReservationFailed>().Any(x => x.ProductId == cmd.ProductId))
                    {
                        eventsToReturn.Add(new Events.ReservationFailed(cmd.ProductId, cmd.TenantId, cmd.CorrelationId));
                    }

                    var previouslyCancelled = events.OfType<Events.ReservationsCancellingNeeded>()
                        .SelectMany(x => x.ProductIdsToCancelReservation);

                    var productIdsToCancelReservations = state.Changes
                        .Where(s => s.ReservedSuccessfuly && !previouslyCancelled.Contains(s.ProductId))
                        .Select(x => x.ProductId)
                        .ToArray();

                    eventsToReturn.Add(new Events.ReservationsCancellingNeeded(state.OrderId,
                        productIdsToCancelReservations, cmd.TenantId, cmd.CorrelationId));

                    return eventsToReturn;
                });
        }
    }
}
