using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Common.Utils;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Enums;
using Application.Contract.Verification.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Verification.Commands;

public sealed class StartVerificationCommandHandler(
    ISmsSenderService sms,
    IEmailSenderService email,
    IVerificationSessionStore store,
    ISmsDailyQuotaService quota,
    IApplicationDbContext db
) : IRequestHandler<StartVerificationCommand, StartVerificationResponse>
{
    private const int CodeLength = 4;
    private const int Attempts = 5;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    public async Task<StartVerificationResponse> Handle(StartVerificationCommand request, CancellationToken ct)
    {
        // ───────── 1. Определяем входные контакты ─────────────────────────────
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(request.Phone);

        if (!hasEmail && !hasPhone)
            throw new BadRequestException("Нужно указать телефон или e-mail.");

        // ───────── 2. Проверяем, что такой пользователь ЕСТЬ (только для reset) ─
        if (request.Purpose.Equals("reset", StringComparison.OrdinalIgnoreCase))
        {
            var userExists = await db.Users.AnyAsync(u =>
                    (hasEmail && u.Email == request.Email) ||
                    (hasPhone && u.PhoneNumber != null &&
                     PhoneMasking.NormalizeE164(u.PhoneNumber) == PhoneMasking.NormalizeE164(request.Phone!)),
                ct);

            if (!userExists)
                throw new NotFoundException("Пользователь не найден.");
        }

        // ───────── 3. Выбираем канал ───────────────────────────────────────────
        var selected = hasEmail ? OtpChannel.email : OtpChannel.phone;

        // === 3. Готовим контакты ===
        string? phoneE164 = null;
        string? emailTo = null;

        if (hasPhone) phoneE164 = PhoneMasking.NormalizeE164(request.Phone!);
        if (hasEmail) emailTo = request.Email;

        // === 4. Проверяем SMS-квоту, если нужно ===
        if (selected == OtpChannel.phone && phoneE164 is not null)
        {
            if (!await quota.TryConsumeAsync(phoneE164, ct))
                throw new BadRequestException("Превышен дневной лимит отправки SMS. Попробуйте завтра.");
        }

        // === 5. Сохраняем сессию ==================================================
        var sessionId = Guid.NewGuid().ToString("N");
        var code = GenerateNumericCode(CodeLength);
        var expiresAt = DateTime.UtcNow.Add(Ttl);

        var session = new VerificationSession(
            SessionId: sessionId,
            Purpose: request.Purpose,
            Selected: selected,
            Phone: phoneE164,
            Email: emailTo,
            Code: code,
            ExpiresAt: expiresAt,
            AttemptsLeft: Attempts,
            NextResendAt: DateTime.UtcNow.Add(Cooldown)
        );

        // === 6. Отправляем сообщение =============================================
        if (selected == OtpChannel.phone)
        {
            var smsText = request.Purpose == "reset"
                ? $"Solnechny-vostok.ru Ваш код для сброса пароля: {code}"
                : $"Solnechny-vostok.ru Ваш код подтверждения: {code}";

            await sms.SendTextAsync("Sol-vostok",
                PhoneMasking.ToSmsIntRecipient(phoneE164!),
                smsText, ct);
        }
        else
        {
            var subj = request.Purpose == "reset"
                ? "Код для сброса пароля (Solnechny-vostok.ru)"
                : "Код подтверждения Solnechny-vostok.ru";

            var plain = request.Purpose == "reset"
                ? $"Ваш код для сброса пароля: {code}"
                : $"Ваш код подтверждения: {code}";

            var html = $"<div style=\"font-family:Arial,sans-serif;font-size:16px\">" +
                       $"{plain.Split(':')[0]}: <b>{code}</b></div>";

            await email.SendAsync(emailTo!, subj, plain, html, ct);
        }

        await store.SaveAsync(session, ct);

        var available = (hasPhone, hasEmail) switch
        {
            (true, true) => new[] { OtpChannel.phone, OtpChannel.email },
            (true, false) => new[] { OtpChannel.phone },
            _ => new[] { OtpChannel.email }
        };

        return new StartVerificationResponse(
            SessionId: sessionId,
            Available: available,
            Selected: selected,
            MaskedPhone: hasPhone ? PhoneMasking.MaskPhoneLast4(phoneE164!) : null,
            MaskedEmail: emailTo,
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
}