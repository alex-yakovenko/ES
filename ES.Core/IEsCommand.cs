namespace ES.Core;

public interface IEsCommand : IMessageContext
{
    public Guid CommandId { get; set; }
    public string CommandName { get; set; }
    public string StreamType { get; set; }

    IEsCommand DeepClone();

    public string AggregateId { get; }
}

public interface IEsCommand<TAggregate> : IEsCommand
    where TAggregate : IAggregateRoot
{
}

public abstract record EsCommand<TAggragate>(string AggregateId) : IEsCommand<TAggragate>
    where TAggragate : IAggregateRoot
{

    public Guid CommandId { get; set; }

    public string CommandName { get; set; } = "!!!NOT SPECIFIED!!!";

    public string StreamType { get; set; } = "!!!NOT SPECIFIED!!!";

    public string? CorrelationId { get; set; }

    public string TenantId { get; set; } = "!!!NOT SPECIFIED!!!";

    public IEsCommand DeepClone()
    {
        return this with { } as IEsCommand;
    }
}
