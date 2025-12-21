using ES.Core;
using ES.Declarations;
using ES.Inventory;
using ES.Orders;
using ES.PlaceOrder;
using ES.UpdateOrder;
using ES.Test.EventStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ES.Test;

public class IntegrationTestBase
{
    protected IServiceProvider? GetServiceProvider(ITestOutputHelper outputHelper, LogLevel debugMinLevel = LogLevel.Information)
    {
        return new ServiceCollection()
            .RegisterInventory()
            .RegisterOrder()
            .RegisterPlaceOrderSaga()
            .RegisterUpdateOrderSaga()

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

            var commandsFlow = serviceProvider.GetRequiredService<IAggregateRunner<TAggregate>>();

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

            var commandsFlow = serviceProvider.GetRequiredService<ISagaRunner<TSaga>>();
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

            var commandsFlow = serviceProvider.GetRequiredService<ISagaRunner<TSaga>>();

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
            CreateCommandsProcessingTask<InventoryItem>(services, InventoryItem.Stream, tenant),
            CreateCommandsProcessingTask<Order>(services, Order.Stream, tenant),

            CreateSagaEventProcessingTask<PlaceOrderSaga>(services, [InventoryItem.Stream, Order.Stream], tenant),
            CreateSagaCommandProcessingTask<PlaceOrderSaga>(services, PlaceOrderSaga.Stream, tenant),

            CreateSagaEventProcessingTask<UpdateOrderSaga>(services, [InventoryItem.Stream, Order.Stream], tenant),
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