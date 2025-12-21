
namespace ES.Core;

public interface IAggregateFlow<TAggregate> where TAggregate : class, IAggregateRoot, new()
{
    Task ProcessCommand<TCommand>(TCommand command) where TCommand : class, IEsCommand<TAggregate>;
}