namespace ES.Core;

public interface IMessageContext
{
    string? CorrelationId { get; }
    string TenantId { get; }
}
