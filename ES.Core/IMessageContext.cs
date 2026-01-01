namespace ES.Core;

public interface IMessageContext
{
    string? CorrelationId { get; }
    string TenantId { get; }
}

public record MessageContext : IMessageContext
{
    public MessageContext() { }

    public MessageContext(string tenantId, string? correlationId = default) 
    {
        TenantId = tenantId;
        CorrelationId = correlationId;
    }

    public MessageContext(IMessageContext context)
    {
        CorrelationId = context.CorrelationId;
        TenantId = context.TenantId;
    }

    public string? CorrelationId { get; set; }

    public string TenantId { get; set; }
}