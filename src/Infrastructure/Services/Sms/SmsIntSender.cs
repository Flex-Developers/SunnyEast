using System.Net.Http.Json;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Sms;

public sealed class SmsIntSender(
    HttpClient http,
    IOptions<SmsIntOptions> options,
    ILogger<SmsIntSender> log) : ISmsSender
{
    private readonly SmsIntOptions _cfg = options.Value;

    public async Task SendAsync(string phone, string text, CancellationToken ct = default)
    {
        if (!_cfg.Enabled)
        {
            log.LogInformation("SMS disabled. [{Phone}] {Text}", phone, text);
            return;
        }

        var payload = new // → именно такой JSON требует SMSint v3
        {
            testMode = _cfg.TestMode,
            messages = new[]
            {
                new
                {
                    sender = _cfg.DefaultSender,
                    recipient = phone,
                    text = text
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, _cfg.Endpoints.SendText)
        {
            Content = JsonContent.Create(payload)
        };
        req.Headers.Add("X-Token", _cfg.Token); // авторизация по токену

        var resp = await http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"SMSint error: {resp.StatusCode} {body}");
    }
}