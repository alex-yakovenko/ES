using ES.Core;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using Eventuous;
using Eventuous.Subscriptions.Context;
using Commands = ES.Declarations.PlaceOrderSaga.Commands;

namespace ES.Test.Sagas
{
    public class PlaceOrderSaga : Eventuous.Subscriptions.EventHandler
    {
        ICommandService<PlaceOrderSagaState> _placeOrderSagaService;
        ICommandService<OrderState> _orderService;
        ICommandService<InventoryState> _inventoryService;

        public PlaceOrderSaga(
            ICommandService<PlaceOrderSagaState> placeOrderSagaService,
            ICommandService<OrderState> orderService,
            ICommandService<InventoryState> inventoryService)
        {
            _placeOrderSagaService = placeOrderSagaService;
            _orderService = orderService;
            _inventoryService = inventoryService;

            On<Declarations.PlaceOrderSaga.Events.Started>(async ctx =>
            {
                var msg = ctx.Message;
                await _orderService.Handle(
                    new Declarations.Orders.Commands.DraftOrder(msg.OrderId,
                        msg.CustomerId, msg.Date, msg.Items,
                        new MessageContext
                        {
                            TenantId = msg.TenantId,
                            CorrelationId = $"PlaceOrderSaga:{msg.SagaId}"
                        }), ctx.CancellationToken);

                foreach (var item in ctx.Message.Items)
                {
                    await _inventoryService.Handle(
                        new Declarations.Inventory.Commands.ReserveProduct(
                            item.ProductId,
                            item.Quantity,
                            ctx.Message.OrderId,
                            ctx.Message), ctx.CancellationToken);
                }
            });

            On<Declarations.Inventory.Events.ProductReserved>(async ctx =>
            {
                if (ctx.Message.ParseCorrelationId().StreamType != "PlaceOrderSaga")
                {
                    return;
                }

                await _placeOrderSagaService.Handle(
                    new Commands.MarkProductReserved(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message), ctx.CancellationToken);
            });

            On<Declarations.Inventory.Events.ProductReservationFailed>(async ctx =>
            {
                if (ctx.Message.ParseCorrelationId().StreamType != "PlaceOrderSaga")
                {
                    return;
                }

                await _placeOrderSagaService.Handle(
                    new Commands.MarkProductReservationFailed(
                        ctx.Message.ParseCorrelationId().SagaId,
                        ctx.Message.ProductId, ctx.Message), ctx.CancellationToken);
            });

            On<Declarations.PlaceOrderSaga.Events.OrderCanBePlaced>(async ctx =>
            {
                await _orderService.Handle(
                    new Declarations.Orders.Commands.SetOrderPlaced(
                        ctx.Message.OrderId, ctx.Message), ctx.CancellationToken);
            });

            On<Declarations.PlaceOrderSaga.Events.OrderNeedsToBeCancelled>(async ctx =>
            {
                await _orderService.Handle(
                    new Declarations.Orders.Commands.CancelOrder(
                        ctx.Message.OrderId, "Failed to reserve products.", ctx.Message), ctx.CancellationToken);

                foreach (var productId in ctx.Message.ProductIds)
                {
                    await _inventoryService.Handle(
                        new Declarations.Inventory.Commands.CancelProductReservation(
                            productId, ctx.Message.OrderId, ctx.Message.Context), ctx.CancellationToken);
                }
            });
        }
    }
}
