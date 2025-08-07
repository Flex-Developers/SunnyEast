using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Utils;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Enums;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Commands;

public sealed class ResendCodeCommandHandler(
    IVerificationSessionStore store,
    ISmsSenderService sms,
    ISmsDailyQuotaService quota,
    IEmailSenderService email
) : IRequestHandler<ResendCodeCommand, ResendResponse>
{
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    public async Task<ResendResponse> Handle(ResendCodeCommand request, CancellationToken ct)
    {
        var s = await store.GetAsync(request.SessionId, ct)
                ?? throw new NotFoundException("Сессия не найдена.");

        if (s.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Сессия истекла.");

        if (s.NextResendAt.HasValue && s.NextResendAt.Value > DateTime.UtcNow)
            throw new BadRequestException("Повторная отправка будет доступна позднее.");

        if (s.Selected == OtpChannel.phone)
        {
            if (string.IsNullOrWhiteSpace(s.Phone))
                throw new BadRequestException("Для этой сессии недоступен канал SMS.");

            var ok = await quota.TryConsumeAsync(s.Phone!, ct);
            if (!ok)
                throw new BadRequestException("Превышен дневной лимит отправки SMS. Попробуйте завтра.");

            var text = $"Ваш код подтверждения для регистрации на сайте Solnechny-vostok.ru: {s.Code}";
            var recipient = PhoneMasking.ToSmsIntRecipient(s.Phone);
            await sms.SendTextAsync("Sol-vostok", recipient, text, ct);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(s.Email))
                throw new BadRequestException("Для этой сессии недоступен канал e-mail.");

            var subject = "Код подтверждения Solnechny-vostok.ru";
            var text    = $"Ваш код подтверждения: {s.Code}";
            var html    = $"<div style=\"font-family:Arial,sans-serif;font-size:16px\">Ваш код подтверждения: <b>{s.Code}</b></div>";
            await email.SendAsync(s.Email, subject, text, html, ct);
        }

        var updated = s with { NextResendAt = DateTime.UtcNow.Add(Cooldown) };
        await store.UpdateAsync(updated, ct);

        return new ResendResponse((int)Cooldown.TotalSeconds);
    }
}
