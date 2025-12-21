namespace ES.Core;

public interface IEsEventCatcher<TSaga>
    where TSaga: ISaga, new()
{
    string CatchedStreamType { get; }
    string CatchedEventType { get; }

    Task<bool> HandleEvent<TEvent>(TEvent @event, TSaga saga)
        where TEvent : class, IEsEvent;
}

public abstract class EsEventCatcher<TSaga, TEvent>(string catchedStreamType) : IEsEventCatcher<TSaga>
    where TEvent : class, IEsEvent
    where TSaga : ISaga, new()
{
    public string CatchedStreamType => catchedStreamType;
    public string CatchedEventType => typeof(TEvent).Name;

    public abstract Task<bool> HandleEvent(TSaga saga, TEvent @event);

    public Task<bool> HandleEvent<TEvent1>(TEvent1 @event, TSaga saga) 
        where TEvent1 : class, IEsEvent
    {
        return HandleEvent(saga, @event as TEvent);
    }
}
