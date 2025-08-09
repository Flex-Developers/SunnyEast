namespace Application.Common.Interfaces.Services;

public interface IEmailSenderService
{
    /// <summary>
    /// Отправляет письмо. Достаточно заполнить textBody или htmlBody.
    /// Бросает исключение при недоступности SMTP.
    /// </summary>
    Task SendAsync(string to, string subject, string? textBody = null, string? htmlBody = null, CancellationToken ct = default);
}