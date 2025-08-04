namespace Application.Common.Interfaces.Services;

public interface ISmsSender
{
    Task SendAsync(string phone, string text, CancellationToken ct = default);
}