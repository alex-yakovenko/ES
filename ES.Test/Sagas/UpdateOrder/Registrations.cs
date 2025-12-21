using ES.Core;
using ES.Test.Sagas.UpdateOrder.Commands;
using ES.Test.Sagas.UpdateOrder.EventCatchers;
using ES.Test.Sagas.UpdateOrder.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace ES.Test.Sagas.UpdateOrder;

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
        .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ProductReservedEventCatcher>();
}
