using ES.Application.EF;
using Eventuous.Subscriptions.Context;
using Microsoft.EntityFrameworkCore;

namespace ES.Application
{
    public class OrdersProjectionBuilder : Eventuous.Subscriptions.EventHandler
    {
        public OrdersProjectionBuilder(ITenantedDbContextFactory dbContextFactory) 
        {
            On<Declarations.Orders.Events.OrderDrafted>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                db.Orders.Add(new EF.Order
                {
                    OrderId = ctx.Message.OrderId,
                    CustomerId = ctx.Message.CustomerId,
                    OrderDate = ctx.Message.Date,
                    Items = [.. ctx.Message.Items
                        .Select(x => new OrderItem
                        {
                            ProductId = x.ProductId,
                            Quantity = x.Quantity
                        })],
                    Status = "Drafted"
                });
                await db.SaveChangesAsync();

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Orders.Events.OrderPlaced>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                var order = await db.Orders.FirstAsync(x => x.OrderId == ctx.Message.OrderId);
                order.Status = "Placed";
                await db.SaveChangesAsync();

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Orders.Events.OrderCanceled>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                var order = await db.Orders.FirstAsync(x => x.OrderId == ctx.Message.OrderId);
                order.Status = "Canceled";
                await db.SaveChangesAsync();

                ctx.Ack(GetType().Name);
            });
        }
    }


}
