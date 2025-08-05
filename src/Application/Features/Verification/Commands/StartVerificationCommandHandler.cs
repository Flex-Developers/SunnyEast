using Application.Common.Interfaces.Services;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Enums;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Commands;

public sealed class StartVerificationCommandHandler(
    ISmsSenderService sms,
    IVerificationSessionStore store
) : IRequestHandler<StartVerificationCommand, StartVerificationResponse>
{
    private const int CodeLength = 4;
    private const int Attempts = 5;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    public async Task<StartVerificationResponse> Handle(StartVerificationCommand request, CancellationToken ct)
    {
        // приоритет — телефон
        var usePhone = !string.IsNullOrWhiteSpace(request.Phone);
        var selected = usePhone ? OtpChannel.phone : OtpChannel.email;

        var sessionId = Guid.NewGuid().ToString("N");
        var code = GenerateNumericCode(CodeLength);

        var expiresAt = DateTime.UtcNow.Add(Ttl);

        var session = new VerificationSession(
            SessionId: sessionId,
            Purpose: request.Purpose,
            Selected: selected,
            Phone: request.Phone,
            Email: request.Email,
            Code: code,
            ExpiresAt: expiresAt,
            AttemptsLeft: Attempts,
            NextResendAt: DateTime.UtcNow.Add(Cooldown)
        );

        // Шлём SMS если телефон задан
        if (selected == OtpChannel.phone && request.Phone is not null)
        {
            var smsText = $"Ваш код подтверждения для регистрации на сайте Solnechny-vostok.ru: {code}";
            var recipient = ToSmsIntRecipientFormat(request.Phone); // "+7 901 123 45 67"
            await sms.SendTextAsync("Sol-vostok", recipient, smsText, ct);
        }

        await store.SaveAsync(session, ct);

        return new StartVerificationResponse(
            SessionId: sessionId,
            Available: usePhone && !string.IsNullOrWhiteSpace(request.Email)
                ? new[] { OtpChannel.phone, OtpChannel.email }
                : usePhone ? new[] { OtpChannel.phone } : new[] { OtpChannel.email },
            Selected: selected,
            MaskedPhone: request.Phone is null ? null : MaskPhone(request.Phone),
            MaskedEmail: request.Email is null ? null : MaskEmail(request.Email),
            CodeLength: CodeLength,
            CooldownSeconds: (int)Cooldown.TotalSeconds,
            TtlSeconds: (int)Ttl.TotalSeconds,
            AttemptsLeft: Attempts
        );
    }

    private static string GenerateNumericCode(int len)
    {
        var rnd = Random.Shared;
        var chars = new char[len];
        for (int i = 0; i < len; i++) chars[i] = (char)('0' + rnd.Next(0, 10));
        return new string(chars);
    }

    private static string ToSmsIntRecipientFormat(string phone) // "+7-901-123-45-67" -> "+7 901 123 45 67"
        => phone.Replace("+7-", "+7 ").Replace("-", " ");

    private static string MaskPhone(string phone) // "+7-901-123-45-67" -> "+7-***-***-**-67"
    {
        var tail = phone[^2..];
        return $"+7-***-***-**-{tail}";
    }

    private static string MaskEmail(string email)
    {
        var i = email.IndexOf('@');
        if (i <= 1) return $"***{email[i..]}";
        return $"{email[..Math.Min(3, i)]}***{email[i..]}";
    }
}