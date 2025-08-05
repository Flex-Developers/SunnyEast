using System.Text;
using System.Text.Json;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class SmsIntService : ISmsSenderService
{
    private readonly HttpClient _http;
    private readonly string _token;
    private readonly ILogger<SmsIntService> _logger;

    public SmsIntService(HttpClient http, IConfiguration cfg, ILogger<SmsIntService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri("https://lcab.smsint.ru/");
        _token = cfg["SmsInt:Token"] ?? throw new InvalidOperationException("SmsInt:Token is not configured");
    }

    public async Task SendTextAsync(string sender, string recipient, string text, CancellationToken ct = default)
    {
        // Собираем JSON строго как в Postman
        var payload = new
        {
            messages = new[]
            {
                new { sender, recipient, text }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "json/v1.0/sms/send/text");
        req.Headers.TryAddWithoutValidation("X-Token", _token);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("SMSInt error HTTP {Status}: {Body}", resp.StatusCode, body);
            throw new InvalidOperationException("Ошибка отправки SMS.");
        }

        _logger.LogInformation("SMSInt response: {Body}", body);
        // По-желанию можно распарсить success:true, но для простоты достаточно кода 200 OK.
    }
}