using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ES.Core;

public static class SagaExtensions
{

    public static (string? StreamType, string? SagaId) ParseCorrelationId(this IMessageContext context)
    {
        if (string.IsNullOrWhiteSpace(context.CorrelationId))
            return (StreamType: null, SagaId: null);

        var parts = context.CorrelationId.Split(':');

        return (
            StreamType: parts[0],
            SagaId: parts.Length > 1 ? parts[1] : null
        );
    }
}
