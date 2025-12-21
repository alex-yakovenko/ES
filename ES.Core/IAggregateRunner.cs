
namespace ES.Core;

public interface IAggregateRunner<TAggregate> where TAggregate : class, IAggregateRoot, new()
{
    Task ProcessCommand<TCommand>(TCommand command) where TCommand : class, IEsCommand<TAggregate>;
}