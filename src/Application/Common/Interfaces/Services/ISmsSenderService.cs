namespace Application.Common.Interfaces.Services;

public interface ISmsSenderService
{
    /// <summary> Отправляет простой текстовый SMS одному получателю. Бросает исключение на ошибке провайдера. </summary>
    Task SendTextAsync(string sender, string recipient, string text, CancellationToken ct = default);
}