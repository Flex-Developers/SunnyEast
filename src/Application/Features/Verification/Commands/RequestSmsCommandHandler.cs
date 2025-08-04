using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Otp;
using Application.Contract.Verification.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Verification.Commands;

public sealed class RequestSmsCommandHandler(
    IApplicationDbContext db,            // либо IApplicationDbContext, если описан
    IOtpService otp,
    ISmsSender sms,
    IOptions<OtpOptions> options) : IRequestHandler<RequestSmsCommand, RequestSmsResult>
{
    private readonly OtpOptions _opt = options.Value;

    public async Task<RequestSmsResult> Handle(RequestSmsCommand request, CancellationToken ct)
    {
        var phone = otp.NormalizePhoneE164(request.Phone);
        var now = DateTimeOffset.UtcNow;

        var last = await db.PhoneOtps
            .Where(x => x.PhoneE164 == phone && x.Purpose == request.Purpose)
            .OrderByDescending(x => x.LastSentAt)
            .FirstOrDefaultAsync(ct);

        if (last?.BlockedUntil is { } blocked && blocked > now)
            throw new InvalidOperationException($"Превышен лимит. Попробуйте после {blocked.LocalDateTime:g}");

        if (last?.LastSentAt is { } sent &&
            (now - sent).TotalSeconds < _opt.ResendCooldownSeconds)
        {
            var left = _opt.ResendCooldownSeconds - (int)(now - sent).TotalSeconds;
            return new RequestSmsResult { CooldownSeconds = left, TtlSeconds = _opt.TtlSeconds };
        }

        var code = otp.GenerateNumericCode(_opt.Length);
        var (hash, salt) = otp.HashCode(code);

        var rec = new PhoneOtp
        {
            PhoneE164 = phone,
            Purpose = request.Purpose,
            CodeHash = hash,
            Salt = salt,
            ExpiresAt = now.AddSeconds(_opt.TtlSeconds),
            Attempts = 0,
            LastSentAt = now,
            BlockedUntil = null
        };

        db.PhoneOtps.Add(rec);
        await db.SaveChangesAsync(ct);

        var text = request.Purpose == "reset"
            ? $"Код для сброса пароля: {code}"
            : $"Код для регистрации: {code}";

        await sms.SendAsync(phone, text, ct);

        return new RequestSmsResult
        {
            CooldownSeconds = _opt.ResendCooldownSeconds,
            TtlSeconds = _opt.TtlSeconds
        };
    }
}