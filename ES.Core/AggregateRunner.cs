using Microsoft.Extensions.Logging;
using System.Linq;

namespace ES.Core;

public class AggregateRunner<TAggregate>(
        IEsEventStorage eventStorage,
        IEnumerable<IEsCommandHandler<TAggregate>> commandHandlers,
        ILogger<AggregateRunner<TAggregate>> logger
    ) : IAggregateRunner<TAggregate> 
    where TAggregate : class, IAggregateRoot, new()
{
    private Dictionary<string, IEsCommandHandler<TAggregate>>? _handlers;

    public virtual async Task ProcessCommand<TCommand>(TCommand command)
        where TCommand : class, IEsCommand<TAggregate>
    {
        logger.LogDebug("Received command of type {type} with content {content}", command.GetType().FullName, command);

        var aggregate = await eventStorage.Load<TAggregate>(command.AggregateId);

        logger.LogDebug("Loaded aggregate of type {aggregate} with content {content}", aggregate.GetType().FullName, aggregate);

        var handler = FindHandler(command.CommandName);

        logger.LogDebug("Found command handler of type {type}", handler.GetType().FullName);

        logger.LogInformation("Handling command {commandName} for aggregate {aggregateId}", command.CommandName, command.AggregateId);
        await handler.Handle(aggregate, command);

        logger.LogDebug("Aggregate after handling {saga}", aggregate);

        if (aggregate.UncommittedEvents.Any())
        {
            logger.LogInformation("Saving {number} new events for {name}: {events}", 
                aggregate.UncommittedEvents.Count,
                aggregate.StreamType,
                string.Join(",", aggregate.UncommittedEvents.Select(x => x.GetType().Name)));

            await eventStorage.SaveEventsAsync(aggregate);
        }
    }

    private IEsCommandHandler<TAggregate> FindHandler(string commandName)
    {
        _handlers ??= commandHandlers
            .ToDictionary(x => x.CommandName, x => x);

        return _handlers[commandName];
    }
}
