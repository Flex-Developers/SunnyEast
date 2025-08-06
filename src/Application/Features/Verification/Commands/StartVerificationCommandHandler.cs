using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Enums;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Commands;

public sealed class StartVerificationCommandHandler(
    ISmsSenderService sms,
    IVerificationSessionStore store,
    ISmsDailyQuotaService quota 
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
        
        // если будет SMS — сначала проверяем квоту
        if (selected == OtpChannel.phone && request.Phone is not null)
        {
            var ok = await quota.TryConsumeAsync(request.Phone, ct);
            if (!ok)
                throw new BadRequestException("Превышен дневной лимит отправки SMS. Попробуйте завтра.");
        }

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
            MaskedEmail: request.Email,
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

    private static string MaskPhone(string phone) // "+7-901-123-45-67" -> "+7-***-***-45-67"
    {
        // Предполагаем формат "+7-XXX-XXX-XX-XX"
        var last2a = phone[^4..^2]; // "45"
        var last2b = phone[^2..];   // "67"
        return $"+7-***-***-{last2a}-{last2b}";
    }
}