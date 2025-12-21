namespace ES.Core;

public abstract class SagaStep<TSaga> : ISagaStep<TSaga>
    where TSaga : ISaga
{
    public virtual string Name => GetType().Name;
    public abstract bool NeedsExecution(TSaga saga); 

    protected bool StepEverExecuted(TSaga saga)
        => saga.StepHistory.Any(sh => sh.Name == Name);

    public abstract Task Execute(TSaga saga);
}
