using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Commands;

public sealed class ResendCodeCommandHandler(
    IVerificationSessionStore store,
    ISmsSenderService sms,
    ISmsDailyQuotaService quota                  // <-- ДОБАВИТЬ
) : IRequestHandler<ResendCodeCommand, ResendResponse>
{
    public async Task<ResendResponse> Handle(ResendCodeCommand request, CancellationToken ct)
    {
        var s = await store.GetAsync(request.SessionId, ct)
                ?? throw new NotFoundException("Сессия не найдена.");

        if (s.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Сессия истекла.");

        if (s.NextResendAt.HasValue && s.NextResendAt.Value > DateTime.UtcNow)
            throw new BadRequestException("Повторная отправка будет доступна позднее.");

        if (string.IsNullOrWhiteSpace(s.Phone))
            throw new BadRequestException("Для этой сессии недоступен канал SMS.");

        // проверка квоты
        var ok = await quota.TryConsumeAsync(s.Phone, ct);
        if (!ok)
            throw new BadRequestException("Превышен дневной лимит отправки SMS. Попробуйте завтра.");

        var text = $"Ваш код подтверждения для регистрации на сайте Solnechny-vostok.ru: {s.Code}";
        var recipient = s.Phone.Replace("+7-", "+7 ").Replace("-", " ");
        await sms.SendTextAsync("Sol-vostok", recipient, text, ct);

        var updated = s with { NextResendAt = DateTime.UtcNow.AddSeconds(60) };
        await store.UpdateAsync(updated, ct);

        return new ResendResponse(60);
    }
}