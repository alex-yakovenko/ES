namespace ES.Core;

public interface ICommandQueue
{
    Task SendCommand(IEsCommand command, CancellationToken cancellationToken = default);
    IAsyncEnumerable<IEsCommand> ConsumeCommands(string tenantId, IEnumerable<string> aggregateNames, int batchSize = 10, int maxTimeoutMs = 5000, CancellationToken cancellationToken = default, string consumerName = "");
}
