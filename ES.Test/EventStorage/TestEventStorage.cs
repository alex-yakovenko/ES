using ES.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ES.Test.EventStorage;

public class TestEventStorage : IEsEventStorage
{
    private readonly ConcurrentQueue<IEsEvent> _events = [];

    private readonly Dictionary<string, int> ConsumerPointers = [];

    public async IAsyncEnumerable<IEsEvent> ConsumeEvents(string tenantId, IEnumerable<string> streamTypes, string? correlationIdPrefix = default, int batchSize = 10, int maxTimeoutMs = 5000, [EnumeratorCancellation] CancellationToken cancellationToken = default, string consumerName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName, nameof(consumerName));

        if (!ConsumerPointers.ContainsKey(consumerName))
            ConsumerPointers[consumerName] = 0;

        var evts = _events.Skip(ConsumerPointers[consumerName]).Take(batchSize);

        foreach (var evt in evts)
        {
            if (evt != null &&
                evt.TenantId == tenantId &&
                streamTypes.Contains(evt.StreamType) &&
                (string.IsNullOrWhiteSpace(correlationIdPrefix) || evt.CorrelationId?.StartsWith(correlationIdPrefix) == true))
            {
                yield return evt.DeepClone();
            }

            ConsumerPointers[consumerName]++;
        }
    }

    public async Task<TAggregate> Load<TAggregate>(string id) 
        where TAggregate : IAggregateRoot, new()
    {
        var agregate = new TAggregate() { Id = id };
        var events = await LoadEventsAsync<TAggregate>(id);

        foreach (var e in events)
        {
            agregate.ApplyEvent(e);
        }

        agregate.UncommittedEvents.Clear();

        return agregate;
    }

    public Task<IEnumerable<IEsEvent>> LoadEventsAsync<TAggregate>(string aggregateId) 
        where TAggregate : IAggregateRoot, new()
    {
        var streamType = new TAggregate().StreamType;

        return Task.FromResult<IEnumerable<IEsEvent>>(
            [.. _events
                .Where(e => e.AggregateId == aggregateId && e.StreamType == streamType)
                .OrderBy(e => e.Version)
                .Select(e => e.DeepClone())]
            );
    }

    public async Task SaveEventsAsync<TAggregate>(TAggregate aggregate)
        where TAggregate : IAggregateRoot
    {
        if (aggregate.UncommittedEvents.Count == 0)
        {
            return;
        }

        var lastSaved = _events
            .Where(e => e.AggregateId == aggregate.Id && e.StreamType == aggregate.UncommittedEvents.First().StreamType)
            .OrderBy(e => e.Version)
            .LastOrDefault();

        var lastSavedVersion = lastSaved?.Version ?? 0;

        if (aggregate.Version.HasValue && lastSavedVersion != aggregate.Version)
        {
            throw new EsConcurrencyException(
                $"Concurrency conflict for aggregate {aggregate.Id} of type {typeof(TAggregate).Name}. " +
                $"Expected version {aggregate.Version.Value}, but last saved version is {lastSavedVersion}."
            );
        }

        foreach (var e in aggregate.UncommittedEvents)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(e.TenantId, nameof(e.TenantId));
            ArgumentException.ThrowIfNullOrWhiteSpace(e.StreamType, nameof(e.StreamType));

            var eventToSave = e.DeepClone();
            eventToSave.EventId = Guid.NewGuid();
            eventToSave.Version = lastSavedVersion++;
            eventToSave.EventType = e.GetType().Name;
            eventToSave.EventTypeVersion = 1; // For simplicity, we set it to 1. In a real scenario, this should be managed properly.
            _events.Enqueue(eventToSave);
        }

        aggregate.Version = lastSavedVersion;

        aggregate.UncommittedEvents.Clear();
    }
}
