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

        if (s.NextResendAt is { } next && next > DateTime.UtcNow)
            throw new BadRequestException("Повторная отправка будет доступна позднее.");

        // --- отправка -------------------------------------------------------------
        if (s.Selected == OtpChannel.phone)
        {
            if (string.IsNullOrWhiteSpace(s.Phone))
                throw new BadRequestException("Для этой сессии недоступен канал SMS.");

            if (!await quota.TryConsumeAsync(s.Phone!, ct))
                throw new BadRequestException("Превышен дневной лимит отправки SMS. Попробуйте завтра.");

            var smsText = s.Purpose == "reset"
                ? $"Solnechny-vostok.ru Ваш код для сброса пароля: {s.Code}"
                : $"Solnechny-vostok.ru Ваш код подтверждения: {s.Code}";

            await sms.SendTextAsync("Sol-vostok",
                PhoneMasking.ToSmsIntRecipient(s.Phone),
                smsText, ct);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(s.Email))
                throw new BadRequestException("Для этой сессии недоступен канал e-mail.");

            var subj = s.Purpose == "reset"
                ? "Код для сброса пароля (Solnechny-vostok.ru)"
                : "Код подтверждения Solnechny-vostok.ru";

            var plain = s.Purpose == "reset"
                ? $"Ваш код для сброса пароля: {s.Code}"
                : $"Ваш код подтверждения: {s.Code}";

            var html = $"<div style=\"font-family:Arial,sans-serif;font-size:16px\">" +
                       $"{plain.Split(':')[0]}: <b>{s.Code}</b></div>";

            await email.SendAsync(s.Email, subj, plain, html, ct);
        }

        await store.UpdateAsync(s with { NextResendAt = DateTime.UtcNow.Add(Cooldown) }, ct);
        return new ResendResponse((int)Cooldown.TotalSeconds);
    }
}