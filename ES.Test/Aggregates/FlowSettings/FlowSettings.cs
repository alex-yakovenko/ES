using ES.Core;

namespace ES.Test.Aggregates.FlowSettings;

public class FlowSettings : AggregateRoot
{
    public HashSet<string> DisabledDommands = [];

    public override string StreamType => FlowSettingsEvents.Stream;

    public void Apply(FlowSettingsEvents.CommandDisabled @event)
    {
        DisabledDommands.Add(@event.CommandTypeName);
    }
    public void Apply(FlowSettingsEvents.CommandEnabled @event)
    {
        DisabledDommands.Remove(@event.CommandTypeName);
    }
}
