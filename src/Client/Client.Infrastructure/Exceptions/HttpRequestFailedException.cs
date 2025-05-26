namespace Client.Infrastructure.Exceptions;

public class HttpRequestFailedException(string message, int? statusCode = null) : Exception(message)
{
    public int? StatusCode { get; } = statusCode;
}