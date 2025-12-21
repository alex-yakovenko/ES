using ES.Core;
using ES.Declarations;
using ES.Inventory.Commands;
using Microsoft.Extensions.DependencyInjection;
using static ES.Declarations.InventoryItem.Commands;

namespace ES.Inventory;

public static class Registrations
{
    public static IServiceCollection RegisterInventory(this IServiceCollection services) => services
        .AddScoped<IEsCommandHandler<InventoryItem>, CreateInventoryItemCommandHandler>()
        .AddScoped<IEsCommandHandler<InventoryItem>, ReserveProductCommandHandler>()
        .AddScoped<IEsCommandHandler<InventoryItem>, ReleaseProductCommandHandler>()
        .AddScoped<IEsCommandHandler<InventoryItem>, CancelProductReservationCommandHandler>()
        .AddScoped<IAggregateRunner<InventoryItem>, AggregateRunner<InventoryItem>>();
}
