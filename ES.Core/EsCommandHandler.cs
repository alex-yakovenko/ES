namespace ES.Core;


public abstract class EsCommandHandler<TCommand, TAggr> : IEsCommandHandler<TAggr, TCommand>
    where TCommand : class, IEsCommand<TAggr>
    where TAggr : class, IAggregateRoot
{
    public string CommandName => typeof(TCommand).Name;
    public abstract Task Handle(TCommand command, TAggr aggregate);


    public async Task Handle<TCustomCommand>(TAggr aggregate, TCustomCommand command) 
        where TCustomCommand : class, IEsCommand<TAggr>
    {
        await Handle(command as TCommand, aggregate);
    }
}
