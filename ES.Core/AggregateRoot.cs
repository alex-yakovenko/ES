using System.Reflection;

namespace ES.Core;

public abstract class AggregateRoot : IAggregateRoot
{
    public string Id { get; set; }

    abstract public string StreamType { get; }
    public int? Version { get; set; }

    public string TenantId { get; protected set; }

    protected Dictionary<string, MethodInfo> ApplyMethods;

    public AggregateRoot()
    {
        ApplyMethods = GetType()
        .GetMethods()
        .Where(x => x.Name == "Apply")
        .Where(x =>
        {
            var prms = x.GetParameters();
            return prms.Length == 1 && prms[0].ParameterType.IsAssignableTo(typeof(IEsEvent));
        })
        .ToDictionary(x => x.GetParameters()[0].ParameterType.Name, x => x);
    }


    public List<IEsEvent> UncommittedEvents { get; } = [];


    public void PushNewEvent(IEsEvent @event)
    {
        @event.CreatedAt = DateTime.UtcNow;
        if (ApplyEvent(@event))
            UncommittedEvents.Add(@event);
        else
            throw new InvalidOperationException($"Unknown event type {@event.EventType}");
    }

    public virtual bool ApplyEvent<TEvent>(TEvent @event) where TEvent : IEsEvent
    {
        var eventTypeName = @event.GetType().Name;

        if (!string.IsNullOrEmpty(TenantId) && @event.TenantId != TenantId)
            return false;

        if (ApplyMethods.TryGetValue(eventTypeName, out var methodInfo))
        {
            methodInfo.Invoke(this, [@event]);
            if (@event.Version != null && 
                @event.AggregateId == Id)
            {
                Version = @event.Version;
            }
            return true;
        }
        else
            return false;
    }
}
