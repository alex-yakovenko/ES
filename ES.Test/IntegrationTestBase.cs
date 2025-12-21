using ES.Core;
using ES.Test.Aggregates.Inventory;
using ES.Test.Aggregates.Inventory.Commands;
using ES.Test.Aggregates.Orders;
using ES.Test.Aggregates.Orders.Commands;
using ES.Test.EventStorage;
using ES.Test.Sagas.PaceOrder;
using ES.Test.Sagas.PaceOrder.Commands;
using ES.Test.Sagas.PaceOrder.EventCatchers;
using ES.Test.Sagas.PaceOrder.Steps;
using ES.Test.Sagas.UpdateOrder;
using ES.Test.Sagas.UpdateOrder.EventCatchers;
using ES.Test.Sagas.UpdateOrder.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ES.Test;

public class IntegrationTestBase
{
    protected IServiceProvider? GetServiceProvider(ITestOutputHelper outputHelper, LogLevel debugMinLevel = LogLevel.Information)
    {
        return new ServiceCollection()
            .AddScoped<IEsCommandHandler<InventoryItem>, CreateInventoryItemCommandHandler>()
            .AddScoped<IEsCommandHandler<InventoryItem>, ReserveProductCommandHandler>()
            .AddScoped<IEsCommandHandler<InventoryItem>, CancelProductReservationCommandHandler>()
            .AddScoped<IAggregateFlow<InventoryItem>, AggregateFlow<InventoryItem>>()

            .AddScoped<IEsCommandHandler<Order>, DraftOrderCommandHandler>()
            .AddScoped<IEsCommandHandler<Order>, SetOrderPlacedCommandHandler>()
            .AddScoped<IEsCommandHandler<Order>, AdjustItemsCommandHandler>()
            .AddScoped<IEsCommandHandler<Order>, CancelOrderCommandHandler>()
            .AddScoped<IAggregateFlow<Order>, AggregateFlow<Order>>()

            .AddScoped<IEsCommandHandler<PlaceOrderSaga>, StartCommandHandler>()
            .AddScoped<ISagaStep<PlaceOrderSaga>, SetOrderPlacedStep>()
            .AddScoped<ISagaStep<PlaceOrderSaga>, CancelOrderStep>()
            .AddScoped<ISagaStep<PlaceOrderSaga>, ReserveProductsStep>()
            .AddScoped<IEsEventCatcher<PlaceOrderSaga>, Sagas.PaceOrder.EventCatchers.ProductReservedEventCatcher>()
            .AddScoped<IEsEventCatcher<PlaceOrderSaga>, Sagas.PaceOrder.EventCatchers.ProductReservationFailedEventCatcher>()
            .AddScoped<ISagaFlowRunner<PlaceOrderSaga>, SagaFlowRunner<PlaceOrderSaga>>()

            .AddScoped<IEsCommandHandler<UpdateOrderSaga>, Sagas.UpdateOrder.Commands.StartCommandHandler> ()
            .AddScoped<ISagaStep<UpdateOrderSaga>, CompleteStep>()
            .AddScoped<ISagaStep<UpdateOrderSaga>, PerformUpdateStep>()
            .AddScoped<ISagaStep<UpdateOrderSaga>, RollbackUpdateStep>()
            .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ItemsAdjustedEventCatcher>()
            .AddScoped<IEsEventCatcher<UpdateOrderSaga>, ItemsAdjustingFialededEventCatcher>()
            .AddScoped<IEsEventCatcher<UpdateOrderSaga>, Sagas.UpdateOrder.EventCatchers.ProductReservationFailedEventCatcher>()
            .AddScoped<IEsEventCatcher<UpdateOrderSaga>, Sagas.UpdateOrder.EventCatchers.ProductReservedEventCatcher>()

            .AddScoped<ISagaFlowRunner<UpdateOrderSaga>, SagaFlowRunner<UpdateOrderSaga>>()


            .AddSingleton<IEsEventStorage, TestEventStorage>()
            .AddSingleton<ICommandQueue, TestCommandQueue>()
            .AddScoped<ISagaStepAllowedVerificationService, SagaStepAllowedVerificationService>()

            .AddLogging(builder => {
                builder.AddDebug();
                builder.AddXUnit(outputHelper);
                builder.SetMinimumLevel(debugMinLevel);
            })

            .BuildServiceProvider();
    }

    protected static (string consumer, string kind, Func<Task<int>> func) CreateCommandsProcessingTask<TAggregate>(
            IServiceProvider services, string streamName, string tenant)
        where TAggregate : class, IAggregateRoot, new()
    {
        var consumerName = new TAggregate().GetType().Name;

        return (consumerName, "agrt-cmds", async () =>
        {
            var counter = 0;

            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var commandsFlow = serviceProvider.GetRequiredService<IAggregateFlow<TAggregate>>();

            var commandQueue = serviceProvider.GetRequiredService<ICommandQueue>();

            await foreach (var cmd in commandQueue.ConsumeCommands(tenant, [streamName], consumerName: consumerName))
            {
                await commandsFlow.ProcessCommand(cmd as IEsCommand<TAggregate>);
                counter++;
            }

            return counter;
        });
    }

    protected static (string consumer, string kind, Func<Task<int>> func) CreateSagaCommandProcessingTask<TSaga>(
            IServiceProvider services, string streamName, string tenant)
        where TSaga : class, ISaga, new()
    {
        var consumerName = new TSaga().GetType().Name;

        return (consumerName, "saga-cmds", async () =>
        {
            var counter = 0;

            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var commandsFlow = serviceProvider.GetRequiredService<ISagaFlowRunner<TSaga>>();
            var commandQueue = serviceProvider.GetRequiredService<ICommandQueue>();

            await foreach (var cmd in commandQueue.ConsumeCommands(tenant, [streamName], consumerName: consumerName))
            {
                await commandsFlow.ProcessCommand(cmd as IEsCommand<TSaga>);
                counter++;
            }

            return counter;
        });
    }

    protected static (string consumer, string kind, Func<Task<int>> func) CreateSagaEventProcessingTask<TSaga>(
            IServiceProvider services, string[] streamNames, string tenant)
        where TSaga : class, ISaga, new()
    {
        var consumerName = new TSaga().GetType().Name;
        return (consumerName, "saga-evt", async () =>
        {
            var counter = 0;

            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var commandsFlow = serviceProvider.GetRequiredService<ISagaFlowRunner<TSaga>>();

            var eventsStorage = serviceProvider.GetRequiredService<IEsEventStorage>();

            await foreach (var evt in eventsStorage.ConsumeEvents(tenant, streamNames, consumerName: consumerName))
            {
                await commandsFlow.ProcessEvent(evt);
                counter++;
            }

            return counter;
        });
    }

    protected static List<(string consumer, string kind, Func<Task<int>> func)> CreateConsumingTasks(IServiceProvider? services, string tenant)
    {
        return [
            CreateCommandsProcessingTask<InventoryItem>(services, InventoryEvents.Stream, tenant),
            CreateCommandsProcessingTask<Order>(services, OrderEvents.Stream, tenant),

            CreateSagaEventProcessingTask<PlaceOrderSaga>(services, [InventoryEvents.Stream, OrderEvents.Stream], tenant),
            CreateSagaCommandProcessingTask<PlaceOrderSaga>(services, PlaceOrderSaga.Stream, tenant),

            CreateSagaEventProcessingTask<UpdateOrderSaga>(services, [InventoryEvents.Stream, OrderEvents.Stream], tenant),
            CreateSagaCommandProcessingTask<UpdateOrderSaga>(services, UpdateOrderSaga.Stream, tenant)
        ];
    }

    protected async Task ProcessQueue(List<(string consumer, string kind, Func<Task<int>> func)> consumingTasks)
    {
        int processedMessages;

        do
        {
            processedMessages = 0;

            foreach (var task in consumingTasks)
                processedMessages += await task.func();

        } while (processedMessages > 0);
    }
}