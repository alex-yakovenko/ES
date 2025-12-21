using ES.Core;
using ES.Orders.Commands;
using Microsoft.Extensions.DependencyInjection;
using ES.Declarations;

namespace ES.Orders;

public static class Registrations
{
    public static IServiceCollection RegisterOrder(this IServiceCollection services) => services
        .AddScoped<IAggregateRunner<Order>, AggregateRunner<Order>>()
        .AddScoped<IEsCommandHandler<Order>, DraftOrderCommandHandler>()
        .AddScoped<IEsCommandHandler<Order>, SetOrderPlacedCommandHandler>()
        .AddScoped<IEsCommandHandler<Order>, AdjustItemsCommandHandler>()
        .AddScoped<IEsCommandHandler<Order>, CancelOrderCommandHandler>();

}
