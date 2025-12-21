namespace ES.Core;

public interface IEsEvent : IMessageContext, IEsMessage
{
    Guid EventId { get; set; }
    int? Version { get; set; }
    string EventType { get; set; }
    int? EventTypeVersion { get; set; }

    DateTime? CreatedAt { get; set; }
}
