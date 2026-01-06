using ES.Application.EF;
using Eventuous.Subscriptions.Context;
using Microsoft.EntityFrameworkCore;

namespace ES.Application
{
    public class InventoryProjectionBuilder : Eventuous.Subscriptions.EventHandler
    {
        public InventoryProjectionBuilder(ITenantedDbContextFactory dbContextFactory)
        {
            On<Declarations.Inventory.Events.InventoryItemCreated>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                db.InventoryItems.Add(new EF.InventoryItem
                {
                    ProductId = ctx.Message.ProductId,
                    AvailableQuantity = ctx.Message.InitialQuantity,
                    Reservations = []
                });

                await db.SaveChangesAsync();

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Inventory.Events.ProductReserved>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);

                var item = await db.InventoryItems.FirstAsync(x => x.ProductId == ctx.Message.ProductId);

                item.Reservations.Add(new ProductReservation
                {
                    OrderId = ctx.Message.OrderId,
                    Quantity = ctx.Message.Quantity,
                    CorrelationId = ctx.Message.CorrelationId!
                });

                item.AvailableQuantity -= ctx.Message.Quantity;

                await db.SaveChangesAsync();

                ctx.Ack(GetType().Name);
            });

            On<Declarations.Inventory.Events.ProductReleased>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                var item = await db.InventoryItems.FirstAsync(x => x.ProductId == ctx.Message.ProductId);

                item.Reservations.Add(new()
                {
                    OrderId = ctx.Message.OrderId,
                    Quantity = -ctx.Message.Quantity,
                    CorrelationId = ctx.Message.CorrelationId
                });

                item.AvailableQuantity += ctx.Message.Quantity;

                await db.SaveChangesAsync();
            });

            On<Declarations.Inventory.Events.ProductReservationCanceled>(async ctx =>
            {
                using var db = dbContextFactory.CreateDbContext(ctx.Message.TenantId);
                var item = await db.InventoryItems.FirstAsync(x => x.ProductId == ctx.Message.ProductId);

                var reservation = item.Reservations.First(x =>
                    x.OrderId == ctx.Message.OrderId &&
                    x.CorrelationId == ctx.Message.CorrelationId);

                item.Reservations.Remove(reservation!);
                item.AvailableQuantity += reservation.Quantity;

                await db.SaveChangesAsync();
            });
        }
    }


}
