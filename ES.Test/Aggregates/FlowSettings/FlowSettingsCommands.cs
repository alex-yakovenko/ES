using ES.Core;

namespace ES.Test.Aggregates.FlowSettings;

public class FlowSettingsCommands
{
    public record DisableCommand(string CommandTypeName) : EsCommand<FlowSettings>("1");
    public record EnableCommand(string CommandTypeName) : EsCommand<FlowSettings>("1");
}
