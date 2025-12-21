namespace ES.Core;

public interface ISaga : IAggregateRoot
{
    HashSet<StepHistoryItem> StepHistory { get; }
}

public record StepHistoryItem (string Name, DateTime ExecutedAt );
