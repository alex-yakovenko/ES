namespace ES.Core;

public interface ISagaStep<TSaga> 
    where TSaga : ISaga
{
    string Name { get; }
    bool NeedsExecution(TSaga saga);
    Task Execute(TSaga saga);
}
