using Application.Common.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Email;

public sealed class MailKitEmailSender(IConfiguration cfg, ILogger<MailKitEmailSender> logger)
    : IEmailSenderService
{
    public async Task SendAsync(string to, string subject, string? textBody = null, string? htmlBody = null,
        CancellationToken ct = default)
    {
        var host     = cfg["Email:SmtpHost"]    ?? throw new InvalidOperationException("Email:SmtpHost not set");
        var port     = int.TryParse(cfg["Email:SmtpPort"], out var p) ? p : 465;
        var useSsl   = !bool.TryParse(cfg["Email:UseSsl"], out var s) || s;
        var username = cfg["Email:Username"]    ?? throw new InvalidOperationException("Email:Username not set");
        var password = cfg["Email:Password"]    ?? throw new InvalidOperationException("Email:Password not set");
        
        
        logger.LogInformation("SMTP ➜ host={Host} port={Port} ssl={Ssl}; username={User}; passwordLoaded={HasPwd}",
            host, port, useSsl, username, !string.IsNullOrEmpty(password));

        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(username));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var builder = new BodyBuilder();
        if (!string.IsNullOrWhiteSpace(htmlBody)) builder.HtmlBody = htmlBody;
        if (!string.IsNullOrWhiteSpace(textBody)) builder.TextBody = textBody;
        msg.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        client.Timeout = 15000; // 15 сек, чтобы не висеть на минуту

        await client.ConnectAsync(
            host, port,
            useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
            ct);

        await client.AuthenticateAsync(username, password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Email sent to {To} with subject {Subj}", to, subject);
    }
}