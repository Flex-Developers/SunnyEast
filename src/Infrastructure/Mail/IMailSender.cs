namespace Infrastructure.Mail;

public interface IMailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default);
}
