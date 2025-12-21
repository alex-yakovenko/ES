using System.Runtime.CompilerServices;

namespace ES.Core;

public static class SagaExtensions
{

    public static string GetCorrelationId(this ISaga saga)
    {
        return $"{saga.StreamType}:{saga.Id:N}";
    }

    public static (string? StreamType, string? SagaId) ParseCorrelationId(this IEsEvent esEvent)
    {
        if (string.IsNullOrWhiteSpace(esEvent.CorrelationId))
            return (StreamType: null, SagaId: null);

        var parts = esEvent.CorrelationId.Split(':');

        return (
            StreamType: parts[0],
            SagaId: parts.Length > 1 ? parts[1] : null
        );
    }

}
