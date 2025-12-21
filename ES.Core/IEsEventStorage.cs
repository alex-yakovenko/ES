namespace ES.Core;

public interface IEsEventStorage
{
    Task<TAggregate> Load<TAggregate>(string id)
        where TAggregate : IAggregateRoot, new();
    Task SaveEventsAsync<TAggregate>(TAggregate aggregate)
        where TAggregate : IAggregateRoot;
    Task<IEnumerable<IEsEvent>> LoadEventsAsync<TAggregate>(string aggregateId)
        where TAggregate : IAggregateRoot, new();

    IAsyncEnumerable<IEsEvent> ConsumeEvents(string tenantId, IEnumerable<string> streamTypes, string? correlationIdPrefix = default, int batchSize = 10, int maxTimeoutMs = 5000, CancellationToken cancellationToken = default, string consumerName = "");
}
