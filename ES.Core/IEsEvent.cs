namespace ES.Core;

public interface IEsEvent : IMessageContext
{
    Guid EventId { get; set; }
    string AggregateId { get; }
    string StreamType { get; }
    int? Version { get; set; }
    string EventType { get; set; }
    int? EventTypeVersion { get; set; }

    string? CorrelationId { get; }

    DateTime? CreatedAt { get; set; }

    new IEsEvent DeepClone();
}
