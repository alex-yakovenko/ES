using ES.Core;
using ES.UpdateOrder.Commands;
using ES.UpdateOrder.EventCatchers;
using ES.UpdateOrder.Steps;
using Microsoft.Extensions.DependencyInjection;
using ES.Declarations;

namespace ES.UpdateOrder;

public static class Registrations
{
    public static IServiceCollection RegisterUpdateOrderSaga(this IServiceCollection services) => services
        .AddScoped<ISagaRunner<UpdateOrderSaga>, SagaRunner<UpdateOrderSaga>>()
        .AddScoped<IEsCommandHandler<UpdateOrderSaga>, StartCommandHandler>()
        .AddScoped<ISagaStep<UpdateOrderSaga>, CompleteStep>()
        .AddScoped<ISagaStep<UpdateOrderSaga>, PerformUpdateStep>()
        .AddScoped<ISagaStep<UpdateOrderSaga>, RollbackUpdateStep>()
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ItemsAdjustedEventCatcher>()
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ItemsAdjustingFialededEventCatcher>()
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ProductReservationFailedEventCatcher>()
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ProductReservedEventCatcher>()
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ProductReleasedEventCatcher>();
}
