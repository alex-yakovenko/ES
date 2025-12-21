namespace ES.Core;

public abstract class Saga: AggregateRoot, ISaga
{
    public HashSet<StepHistoryItem> StepHistory { get; } = [];

    public override bool ApplyEvent<TEvent>(TEvent @event)
    {
        if (@event.CorrelationId != this.GetCorrelationId())
            throw new InvalidDataException($"Saga can process only events with CorrelationId = saga.Id. Wrong event with ID {@event.EventId}");

        return base.ApplyEvent(@event);
    }

    public void Apply(CommonSagaEvents.StepExecuted @event)
    {
        StepHistory.Add(new StepHistoryItem(@event.StepName, @event.ExecutedAt));
    }

}
