namespace Application.Common.Exceptions;

public class ExistException : Exception
{
    public ExistException()
    {
    }

    public ExistException(string message) : base(message)
    {
    }
}