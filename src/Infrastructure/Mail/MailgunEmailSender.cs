using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Mail;

public class MailgunEmailSender : IMailSender
{
    private readonly HttpClient _httpClient;
    private readonly MailgunOptions _options;
    private readonly ILogger<MailgunEmailSender> _logger;

    public MailgunEmailSender(HttpClient httpClient, IOptions<MailgunOptions> options, ILogger<MailgunEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_options.ApiKey}"));
        _httpClient.BaseAddress = new Uri(_options.BaseUri.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    }

    public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent($"{_options.FromName} <{_options.Sender}>", Encoding.UTF8), "from" },
            { new StringContent(to, Encoding.UTF8), "to" },
            { new StringContent(subject, Encoding.UTF8), "subject" },
            { new StringContent(htmlBody, Encoding.UTF8), "html" }
        };

        if (!string.IsNullOrEmpty(textBody))
            content.Add(new StringContent(textBody), "text");

        try
        {
            var response = await _httpClient.PostAsync($"{_options.Domain}/messages", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Mailgun send failed: {Status} {Body}", response.StatusCode, body);
                throw new MailgunException("Failed to send email");
            }

            _logger.LogInformation("Mail sent to {Email}", to);
        }
        catch (Exception ex) when (ex is not MailgunException)
        {
            _logger.LogError(ex, "Error sending email");
            throw new MailgunException("Failed to send email", ex);
        }
    }
}

public class MailgunException : Exception
{
    public MailgunException(string message) : base(message) { }
    public MailgunException(string message, Exception inner) : base(message, inner) { }
}
