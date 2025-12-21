namespace ES.Core;

public interface IEsMessage
{
    string StreamType { get; set; }
    public string AggregateId { get; }
}