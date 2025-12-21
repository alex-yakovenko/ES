namespace ES.Core;

public static class CommonSagaEvents
{
    public record StepExecuted(
        string AggregateId,
        string StepName,
        DateTime ExecutedAt
    ) : EsEvent(AggregateId, "");
}
