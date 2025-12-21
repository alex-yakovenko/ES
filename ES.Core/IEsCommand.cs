namespace ES.Core;

public interface IEsCommand : IMessageContext, IEsMessage
{
    public Guid CommandId { get; set; }
    public string CommandName { get; set; }

}

public interface IEsCommand<TAggregate> : IEsCommand
    where TAggregate : IAggregateRoot
{
}
