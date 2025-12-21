using ES.Core;
using ES.PlaceOrder.Commands;
using ES.PlaceOrder.EventCatchers;
using ES.PlaceOrder.Steps;
using Microsoft.Extensions.DependencyInjection;
using ES.Declarations;

namespace ES.PlaceOrder;

public static class Registrations
{
    public static IServiceCollection RegisterPlaceOrderSaga(this IServiceCollection services) => services
        .AddScoped<ISagaRunner<PlaceOrderSaga>, SagaRunner<PlaceOrderSaga>>()
        .AddScoped<IEsCommandHandler<PlaceOrderSaga>, StartCommandHandler>()
        .AddScoped<ISagaStep<PlaceOrderSaga>, SetOrderPlacedStep>()
        .AddScoped<ISagaStep<PlaceOrderSaga>, CancelOrderStep>()
        .AddScoped<ISagaStep<PlaceOrderSaga>, ReserveProductsStep>()
        .AddScoped<IEsEventCatcher<PlaceOrderSaga>, ProductReservedEventCatcher>()
        .AddScoped<IEsEventCatcher<PlaceOrderSaga>, ProductReservationFailedEventCatcher>();
}
