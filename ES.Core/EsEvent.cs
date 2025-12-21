namespace ES.Core;

public record EsEvent (string AggregateId, string StreamType) : IEsEvent
{
    public Guid EventId { get; set; }
    public int? Version { get; set; }
    public string? CorrelationId { get; set; }
    public string TenantId { get; set; } = "!!!NO TENANT SPECIFIED!!!";
    public string EventType { get; set; } = "";
    public int? EventTypeVersion { get; set; }
    public DateTime? CreatedAt { get; set; }

    IEsEvent IEsEvent.DeepClone()
    {
        return this with { };
    }
}
