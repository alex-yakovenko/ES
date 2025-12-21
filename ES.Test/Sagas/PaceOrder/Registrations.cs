using ES.Core;
using ES.Test.Sagas.PaceOrder.Commands;
using ES.Test.Sagas.PaceOrder.EventCatchers;
using ES.Test.Sagas.PaceOrder.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace ES.Test.Sagas.PaceOrder;

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
