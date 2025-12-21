namespace ES.Core;

public interface IEsCommandHandler<TAggregate>
    where TAggregate : IAggregateRoot
{
    string CommandName { get; }
    Task Handle<TCommand>(TAggregate aggregate, TCommand command)
        where TCommand : class, IEsCommand<TAggregate>;
}

public interface IEsCommandHandler<TAggregate, TCommand> : IEsCommandHandler<TAggregate>
    where TCommand : class, IEsCommand<TAggregate>
    where TAggregate : IAggregateRoot
{
    Task Handle(TCommand command, TAggregate aggregate);            
}
