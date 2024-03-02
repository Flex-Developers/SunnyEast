using System.Net;

namespace Client.Infrastructure.Services.HttpClient;

public class ServerResponse
{
    public required bool Success { get; set; }
    public required HttpStatusCode? StatusCode { get; set; }
}

public class ServerResponse<T> : ServerResponse
{
    public T? Response { get; set; }
}