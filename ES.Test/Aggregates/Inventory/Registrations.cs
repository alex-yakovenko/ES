using ES.Core;
using ES.Test.Aggregates.Inventory.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ES.Test.Aggregates.Inventory;

public static class Registrations
{
    public static IServiceCollection RegisterInventory(this IServiceCollection services) => services
        .AddScoped<IEsCommandHandler<InventoryItem>, CreateInventoryItemCommandHandler>()
        .AddScoped<IEsCommandHandler<InventoryItem>, ReserveProductCommandHandler>()
        .AddScoped<IEsCommandHandler<InventoryItem>, CancelProductReservationCommandHandler>()
        .AddScoped<IAggregateRunner<InventoryItem>, AggregateRunner<InventoryItem>>();
}
