namespace ES.Core;

public interface IAggregateRoot
{
    string Id { get; set; }
    string TenantId { get; }
    List<IEsEvent> UncommittedEvents { get; }
    bool ApplyEvent<TEvent>(TEvent @event) where TEvent : IEsEvent;
    void PushNewEvent(IEsEvent @event);
    string StreamType { get; }
    int? Version { get; set; }
}
