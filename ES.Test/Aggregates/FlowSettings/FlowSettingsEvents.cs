using ES.Core;

namespace ES.Test.Aggregates.FlowSettings;

public class FlowSettingsEvents
{
    public const string Stream = "flow_settings";
    public record CommandDisabled(string CommandTypeName) : EsEvent("1", Stream);
    public record CommandEnabled(string CommandTypeName) : EsEvent("1", Stream);
}
