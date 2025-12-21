
namespace ES.Core;

public interface ISagaRunner<TSaga> where TSaga : ISaga, new()
{
    Task ProcessCommand<TCommand>(TCommand command) 
        where TCommand : class, IEsCommand<TSaga>;
    Task ProcessEvent<TEvent>(TEvent @event)
        where TEvent : class, IEsEvent;
}
