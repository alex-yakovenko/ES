namespace ES.Core;

public abstract record EsCommand<TAggragate>(string AggregateId) : IEsCommand<TAggragate>
    where TAggragate : IAggregateRoot
{

    public Guid CommandId { get; set; }

    public string CommandName { get; set; } = "!!!NOT SPECIFIED!!!";

    public string StreamType { get; set; } = "!!!NOT SPECIFIED!!!";

    public string? CorrelationId { get; set; }

    public string TenantId { get; set; } = "!!!NOT SPECIFIED!!!";
}
