using ES.Declarations.PlaceOrderSaga;
using ES.Core;
using Eventuous;
using System.Linq;
using System.Collections.Generic;

namespace ES.Application.Sagas
{
    public class PlaceOrderSagaService : CommandService<PlaceOrderSagaState>
    {
        public PlaceOrderSagaService(IEventReader reader, IEventWriter writer) : base(reader, writer)
        {
            On<Commands.Start>()
                .InState(ExpectedState.New)
                .GetStream(cmd => new StreamName($"PlaceOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                [
                    new Events.Started(cmd.SagaId, cmd.OrderId, cmd.CustomerId,
                        cmd.Date, cmd.Items,
                        cmd.TenantId, $"PlaceOrderSaga:{cmd.SagaId}")
                ]);

            On<Commands.MarkProductReserved>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"PlaceOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    if (events.OfType<Events.ProductReserved>()
                        .Any(x => x.ProductId == cmd.ProductId && x.CorrelationId == cmd.CorrelationId))
                    {
                        // Idempotency check - reservation for this product already exists
                        return [];
                    }

                    var eventsToReturn = new List<object>
                        { new Events.ProductReserved(cmd.ProductId, cmd.TenantId, cmd.CorrelationId) };

                    var allSatisfied = state.ItemsToReserve.All(x =>
                        x.ProductId == cmd.ProductId
                            ? !x.ReservationFailed
                            : (x.ProductReserved && !x.ReservationFailed)
                    );

                    if (allSatisfied && !state.OrderCanBePlaced && !events.OfType<Events.OrderCanBePlaced>()
                        .Any(x => x.CorrelationId == cmd.CorrelationId))
                    {
                        eventsToReturn.Add(new Events.OrderCanBePlaced(state.OrderId, cmd.TenantId, cmd.CorrelationId));
                    }

                    return eventsToReturn;
                });

            On<Commands.MarkProductReservationFailed>()
                .InState(ExpectedState.Existing)
                .GetStream(cmd => new StreamName($"PlaceOrderSaga-{cmd.SagaId}"))
                .Act((state, events, cmd) =>
                {
                    if (events.OfType<Events.ProductReservationFailed>()
                        .Any(x => x.ProductId == cmd.ProductId && x.CorrelationId == cmd.CorrelationId))
                    {
                        // Idempotency check - reservation failure for this product already exists
                        return [];
                    }

                    var eventsToReturn = new List<object>
                        { new Events.ProductReservationFailed(cmd.ProductId, cmd.TenantId, cmd.CorrelationId) };

                    if (!state.OrderNeedsToBeCancelled)
                    {
                        var reservationsToCancel = state.ItemsToReserve
                            .Where(x => !x.ReservationFailed && x.ProductId != cmd.ProductId)
                            .Select(x => x.ProductId)
                            .ToList();

                        if (!events.OfType<Events.OrderNeedsToBeCancelled>()
                            .Any(x => x.CorrelationId == cmd.CorrelationId))
                        {
                            eventsToReturn.Add(
                                new Events.OrderNeedsToBeCancelled(state.OrderId, reservationsToCancel, cmd.TenantId,
                                    cmd.CorrelationId));
                        }
                    }

                    return eventsToReturn;
                });
        }
    }
}
