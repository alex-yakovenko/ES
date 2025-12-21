using ES.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ES.Test.EventStorage;

public class TestCommandQueue : ICommandQueue
{
    private readonly ConcurrentQueue<IEsCommand> _commads = [];

    private readonly Dictionary<string, int> ConsumerPointers = [];

    public async IAsyncEnumerable<IEsCommand> ConsumeCommands(string tenantId, IEnumerable<string> aggregateNames, int batchSize = 10, int maxTimeoutMs = 5000, [EnumeratorCancellation] CancellationToken cancellationToken = default, string consumerName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName, nameof(consumerName));

        if (!ConsumerPointers.ContainsKey(consumerName))
            ConsumerPointers[consumerName] = 0;

        var cmds = _commads.Skip(ConsumerPointers[consumerName]).Take(batchSize);

        foreach (var cmd in cmds)
        {
            if (cmd != null && cmd.TenantId == tenantId &&
                aggregateNames.Contains(cmd.StreamType))
            {
                yield return cmd.JsonClone();
            }

            ConsumerPointers[consumerName]++;
        }
    }

    public async Task SendCommand(IEsCommand command, CancellationToken cancellationToken = default)
    {
        var commandToSend = command.JsonClone();
        commandToSend.CommandId = Guid.NewGuid();
        commandToSend.CommandName = command.GetType().Name;
        commandToSend.StreamType = GetStreamType(command.GetType());
        _commads.Enqueue(commandToSend);
    }

    private string GetStreamType(Type type)
    {
        var aggregateType = type.GetInterfaces()
            .First(x => x.Name.StartsWith("IEsCommand") && x.GenericTypeArguments.Length == 1)
            .GenericTypeArguments[0];

        var instance = Activator.CreateInstance(aggregateType) as IAggregateRoot;

        return instance.StreamType;
    }
}
