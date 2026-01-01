using ES.Core;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Eventuous.Subscriptions.Context;

namespace ES.Test.Sagas
{
    public class UpdateOrderSaga : Eventuous.Subscriptions.EventHandler
    {
        private readonly ICommandService<Declarations.Inventory.InventoryState> _inventoryService;
        private readonly ICommandService<Declarations.UpdateOrderSaga.UpdateOrderSagaState> _updateOrderSagaService;

        public UpdateOrderSaga(
            ICommandService<Declarations.Inventory.InventoryState> inventoryService,
            ICommandService<Declarations.UpdateOrderSaga.UpdateOrderSagaState> updateOrderSagaService)
        {
            _inventoryService = inventoryService;
            _updateOrderSagaService = updateOrderSagaService;

            On<Declarations.UpdateOrderSaga.Events.Started>(async ctx => 
            {
                var evt = ctx.Message;
                foreach (var product in evt.Changes)
                    if (product.AdjustQuantityBy > 0)
                    {
                        await _inventoryService.Handle(new Declarations.Inventory.Commands.ReserveProduct(
                            product.ProductId, product.AdjustQuantityBy, evt.OrderId, evt), ctx.CancellationToken);
                    }
                    else
                    {
                        await _inventoryService.Handle(new Declarations.Inventory.Commands.ReleaseProduct(
                            product.ProductId, - product.AdjustQuantityBy, evt.OrderId, evt), ctx.CancellationToken);
                    }
            });

            On<Declarations.Orders.Events.ItemsAdjusted>(async ctx => 
            {
                var evt = ctx.Message;

                if (evt.ParseCorrelationId().StreamType != UpdateOrderSagaAggregate.Name)
                {
                    ctx.Ignore(nameof(Declarations.Orders.Events.ItemsAdjusted));
                    return;
                }

                var cmd = new Declarations.UpdateOrderSaga.Commands.UpdateStatus(evt.ParseCorrelationId().SagaId, true, evt);
                
                await _updateOrderSagaService.Handle(cmd, ctx.CancellationToken);
            });

            On<Declarations.Inventory.Events.ProductReserved>(async ctx =>
            {

                if (ctx.Message.ParseCorrelationId().StreamType != UpdateOrderSagaAggregate.Name)
                {
                    //ctx.Ignore(nameof(Declarations.Inventory.Events.ProductReserved));
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Commands.MarkProductReservedOrReleased(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message), ctx.CancellationToken);
            });

            On<Declarations.Inventory.Events.ProductReleased>(async ctx =>
            {

                if (ctx.Message.ParseCorrelationId().StreamType != UpdateOrderSagaAggregate.Name)
                {
                    ctx.Ignore(nameof(Declarations.Inventory.Events.ProductReserved));
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Commands.MarkProductReservedOrReleased(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message), ctx.CancellationToken);
            });

            On<Declarations.Inventory.Events.ProductReservationFailed>(async ctx =>
            {

                if (ctx.Message.ParseCorrelationId().StreamType != UpdateOrderSagaAggregate.Name)
                {
                    ctx.Ignore(nameof(Declarations.Inventory.Events.ProductReservationFailed));
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Declarations.UpdateOrderSaga.Commands.MarkProductReservationFailed(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message), ctx.CancellationToken);
            });

            On<Events.SetResult>(async ctx => 
            {
                var evt = ctx.Message;
                foreach (var productId in evt.ProductIdsToCancelReservations)
                {
                    await inventoryService.Handle(new Declarations.Inventory.Commands.CancelProductReservation(
                        productId, evt.OrderId, evt), ctx.CancellationToken);
                }
            });
        }

    }
}
