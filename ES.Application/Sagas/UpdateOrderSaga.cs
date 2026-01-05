using ES.Core;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Eventuous.Subscriptions.Context;

namespace ES.Application.Sagas
{
    public class UpdateOrderSaga : Eventuous.Subscriptions.EventHandler
    {
        private readonly ICommandService<Declarations.Inventory.InventoryState> _inventoryService;
        private readonly ICommandService<Declarations.Orders.OrderState> _orderService;
        private readonly ICommandService<Declarations.UpdateOrderSaga.UpdateOrderSagaState> _updateOrderSagaService;

        public UpdateOrderSaga(
            ICommandService<Declarations.Inventory.InventoryState> inventoryService,
            ICommandService<Declarations.UpdateOrderSaga.UpdateOrderSagaState> updateOrderSagaService,
            ICommandService<Declarations.Orders.OrderState> orderService)
        {
            _inventoryService = inventoryService;
            _updateOrderSagaService = updateOrderSagaService;
            _orderService = orderService;

            On<Declarations.UpdateOrderSaga.Events.Started>(async ctx =>
            {
                var evt = ctx.Message;
                foreach (var product in evt.Changes)
                    if (product.AdjustQuantityBy > 0)
                    {
                        await _inventoryService.Handle(new Declarations.Inventory.Commands.ReserveProduct(
                                product.ProductId, product.AdjustQuantityBy, evt.OrderId, evt.TenantId,
                                evt.CorrelationId),
                            ctx.CancellationToken);
                    }
                    else
                    {
                        await _inventoryService.Handle(new Declarations.Inventory.Commands.ReleaseProduct(
                                product.ProductId, -product.AdjustQuantityBy, evt.OrderId, evt.TenantId,
                                evt.CorrelationId),
                            ctx.CancellationToken);
                    }

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Orders.Events.ItemsAdjusted>(async ctx =>
            {
                var evt = ctx.Message;

                if (evt.ParseCorrelationId().StreamType != "UpdateOrderSaga")
                {
                    ctx.Ignore(GetType().Name);
                    return;
                }

                var cmd = new Declarations.UpdateOrderSaga.Commands.UpdateStatus(
                    evt.ParseCorrelationId().SagaId, true, evt.TenantId, evt.CorrelationId);

                await _updateOrderSagaService.Handle(cmd, ctx.CancellationToken);

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Orders.Events.ItemsAdjustingFialeded>(async ctx =>
            {
                var evt = ctx.Message;

                if (evt.ParseCorrelationId().StreamType != "UpdateOrderSaga")
                {
                    ctx.Ignore(GetType().Name);
                    return;
                }

                var cmd = new Declarations.UpdateOrderSaga.Commands.UpdateStatus(
                    evt.ParseCorrelationId().SagaId, false, evt.TenantId, evt.CorrelationId);

                await _updateOrderSagaService.Handle(cmd, ctx.CancellationToken);

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Inventory.Events.ProductReserved>(async ctx =>
            {
                if (ctx.Message.ParseCorrelationId().StreamType != "UpdateOrderSaga")
                {
                    ctx.Ignore(GetType().Name);
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Commands.MarkProductReservedOrReleased(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message.TenantId, ctx.Message.CorrelationId), ctx.CancellationToken);
                
                ctx.Ack(GetType().Name);
            });

            On<Declarations.Inventory.Events.ProductReleased>(async ctx =>
            {
                if (ctx.Message.ParseCorrelationId().StreamType != "UpdateOrderSaga")
                {
                    ctx.Ignore(GetType().Name);
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Commands.MarkProductReservedOrReleased(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message.TenantId, ctx.Message.CorrelationId), ctx.CancellationToken);

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Inventory.Events.ProductReservationFailed>(async ctx =>
            {
                if (ctx.Message.ParseCorrelationId().StreamType != "UpdateOrderSaga")
                {
                    ctx.Ignore(GetType().Name);
                    return;
                }

                await _updateOrderSagaService.Handle(
                    new Declarations.UpdateOrderSaga.Commands.MarkProductReservationFailed1(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message.TenantId, ctx.Message.CorrelationId), ctx.CancellationToken);

                ctx.Ack(GetType().Name);
            });

            On<Events.AllProductsReserved>(async ctx =>
            {
                var evt = ctx.Message;
                var changes = evt.Changes
                    .Select(x => new Declarations.Orders.OrderItemChange(
                        x.ProductId, x.AdjustQuantityBy))
                    .ToArray();

                await orderService.Handle(new Declarations.Orders.Commands.AdjustProducts(
                    evt.OrderId, changes, evt.TenantId, evt.CorrelationId), ctx.CancellationToken);

                ctx.Ack(GetType().Name);
            });

            On<Events.ReservationsCancellingNeeded>(async ctx =>
            {
                var evt = ctx.Message;
                foreach (var productId in evt.ProductIdsToCancelReservation)
                {
                    await inventoryService.Handle(new Declarations.Inventory.Commands.CancelProductReservation(
                        productId, evt.OrderId, evt.TenantId, evt.CorrelationId), ctx.CancellationToken);
                }

                ctx.Ack(GetType().Name);
            });
        }
    }
}
