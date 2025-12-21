namespace ES.Core;

public class EsConcurrencyException : Exception
{
    public EsConcurrencyException(string message) : base(message)
    {
    }
}
