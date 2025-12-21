using Microsoft.Extensions.Logging;

namespace ES.Core;

public class SagaRunner<TSaga>(
    IEnumerable<ISagaStep<TSaga>> steps,
    IEsEventStorage eventStorage,
    ISagaStepAllowedVerificationService stepAllowedVerificationService,
    IEnumerable<IEsCommandHandler<TSaga>> commandHandlers,
    IEnumerable<IEsEventCatcher<TSaga>> eventCatchers,
    ILogger<SagaRunner<TSaga>> logger
    ) : ISagaRunner<TSaga> where TSaga : ISaga, new()
{
    private Dictionary<string, IEsCommandHandler<TSaga>>? _handlers;
    private Dictionary<string, IEsEventCatcher<TSaga>>? _catchers;

    public async Task ProcessEvent<TEvent>(TEvent @event)
    where TEvent : class, IEsEvent
    {
        logger.LogDebug("Received event of type {type} with content {content}", @event.GetType().FullName, @event);
        var correlationInfo = @event.ParseCorrelationId();

        logger.LogDebug("Detected saga Id: {sagaId}", correlationInfo.SagaId);

        if (correlationInfo.StreamType != new TSaga().StreamType)
        {
            logger.LogDebug("Skipped event due to inappropriate saga type");
            return;
        }

        TSaga? saga = await eventStorage.Load<TSaga>(correlationInfo.SagaId);

        logger.LogDebug("Loaded saga of type {type} with content {content}", saga.GetType().FullName, saga);

        var eventCatcher = FindEventCatcher(@event.EventType, @event.StreamType);

        if (eventCatcher == null)
        {
            logger.LogDebug("Failed to find a catcher for {streamType}.{eventType}", @event.StreamType, @event.EventType);
            return;
        }

        logger.LogDebug("Found event catcher of type {type}", eventCatcher.GetType().FullName);

        logger.LogInformation("Handling event {eventType} for saga {sagaId}", @event.EventType, correlationInfo.SagaId);
        await eventCatcher.HandleEvent(@event, saga);

        logger.LogDebug("Saga after handling {saga}", saga);

        await eventStorage.SaveEventsAsync(saga);

        saga = await eventStorage.Load<TSaga>(correlationInfo.SagaId);

        await RunNextStep(saga);
    }

    public async Task ProcessCommand<TCommand>(TCommand command)
        where TCommand : class, IEsCommand<TSaga>
    {
        logger.LogDebug("Received command of type {type} with content {content}", command.GetType().FullName, command);

        if (command.StreamType != new TSaga().StreamType)
        {
            logger.LogDebug("Skipped event due to inappropriate saga type");
            return;
        }

        var saga = await eventStorage.Load<TSaga>(command.AggregateId);

        logger.LogDebug("Loaded saga of type {type} with content {content}", saga.GetType().FullName, saga);

        IEsCommandHandler<TSaga> commandHandler = FindCommandHandler(command.CommandName);

        logger.LogDebug("Found command handler of type {type}", commandHandler.GetType().FullName);

        logger.LogInformation("Handling command {commandName} for saga {sagaId}", command.CommandName, command.AggregateId);
        await commandHandler.Handle(saga, command);

        logger.LogDebug("Saga after handling {saga}", saga);

        logger.LogInformation("Saving {number} new events", saga.UncommittedEvents.Count);

        await eventStorage.SaveEventsAsync(saga);

        saga = await eventStorage.Load<TSaga>(command.AggregateId);

        await RunNextStep(saga);
    }

    private IEsCommandHandler<TSaga> FindCommandHandler(string commandName)
    {
        _handlers ??= commandHandlers
            .ToDictionary(x => x.CommandName, x => x);

        return _handlers[commandName];
    }


    private IEsEventCatcher<TSaga>? FindEventCatcher(string eventType, string streamType)
    {
        _catchers ??= eventCatchers
            .ToDictionary(x => $"{x.CatchedStreamType}:{x.CatchedEventType}", x => x);

        return _catchers.TryGetValue($"{streamType}:{eventType}", out var catcher)
            ? catcher
            : null;
    }


    private async Task RunNextStep(TSaga saga)
    {

        foreach (var step in steps)
        {
            if (!step.NeedsExecution(saga))
            {
                logger.LogDebug("Step {stepName} doesn't need to be executed.", step.Name);
                continue;
            }

            var isAllowed = await stepAllowedVerificationService.IsStepAllowedAsync(saga, step.Name);

            if (!isAllowed)
            {
                logger.LogDebug("Step {stepName} needs to be executed, but it is disabled.", step.Name);
                continue;
            }

            logger.LogInformation("Executing step {stepName} for saga {sagaId}.", step.Name, saga.Id);
            await step.Execute(saga);

            logger.LogDebug("Apply event about step {stepName} execution.", step.Name);
            saga.PushNewEvent(new CommonSagaEvents.StepExecuted(saga.Id, step.Name, DateTime.UtcNow)
            {
                CorrelationId = saga.GetCorrelationId(),
                TenantId = saga.TenantId,
                StreamType = saga.StreamType
            });

            logger.LogDebug("Save {number} new events to event storage", saga.UncommittedEvents.Count);
            await eventStorage.SaveEventsAsync(saga);
        }
    }
}
