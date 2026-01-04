using ES.Core;
using ES.Declarations;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using ES.Declarations.UpdateOrderSaga;
using ES.Test.Aggregates;
using ES.Test.EventStorage;
using ES.Test.Sagas;
using Eventuous;
using Eventuous.Subscriptions;
using Eventuous.Subscriptions.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Eventuous.Diagnostics.Logging;
using Xunit.Abstractions;
using Microsoft.Extensions.Hosting;

namespace ES.Test;

public class IntegrationTestBase : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    protected IServiceProvider GetServiceProvider(ITestOutputHelper outputHelper,
        LogLevel debugMinLevel = LogLevel.Information)
    {
        var services = new ServiceCollection()
            .AddCommandService<InventoryService, InventoryState>()
            .AddCommandService<OrderService, OrderState>()
            .AddCommandService<PlaceOrderSagaService, PlaceOrderSagaState>()
            .AddCommandService<UpdateOrderSagaService, UpdateOrderSagaState>()
            .AddEventStore<TestEventStore>()
            .AddScoped<IEventHandler, PlaceOrderSaga>()
            .AddScoped<IEventHandler, UpdateOrderSaga>()

            .AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddXUnit(outputHelper);
                builder.SetMinimumLevel(debugMinLevel);
            })
            .BuildServiceProvider();

        services.AddEventuousLogs(System.Diagnostics.Tracing.EventLevel.LogAlways);

        var listener = new LoggingEventListener(services.GetRequiredService<ILoggerFactory>());
        _disposables.Add(listener);
        _disposables.Add(services);

        return services;
    }

    protected Func<Task> CreateHandlerRunner(IServiceProvider services, Dictionary<string, int> readPositions,
        string tenant)
    {
        var eventReader = services.GetRequiredService<IEventReader>();

        var handlers = services.GetServices<IEventHandler>()
            .Select(x => (Func<Task<int>>)(async () =>
            {
                var handlerName = x.GetType().Name;
                if (!readPositions.ContainsKey(handlerName))
                    readPositions[handlerName] = 0;

                int totalEventsRead = 0;

                var start = readPositions[handlerName];
                StreamEvent[] events;
                do
                {
                    events = await eventReader.ReadEvents(new StreamName("-"),
                        new StreamReadPosition(start), 10, CancellationToken.None);
                    totalEventsRead += events.Length;

                    foreach (var evt in events)
                    {
                        var ctx = new MessageConsumeContext("", "", "", "-", 0,
                            0, 0, 0, default, evt.Payload, evt.Metadata, "", CancellationToken.None);
                        var result = await x.HandleEvent(ctx);
                        start++;
                    }
                } while (events.Length > 0);

                readPositions[handlerName] = start;

                return totalEventsRead;
            }))
            .ToArray();

        return async () =>
        {
            int totalEventsRead = 0;
            do
            {
                totalEventsRead = 0;
                foreach (var handler in handlers)
                {
                    totalEventsRead += await handler();
                }
            } while (totalEventsRead > 0);
        };
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}