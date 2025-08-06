using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Utils;
using Application.Contract.Verification.Enums;
using Application.Contract.Verification.Queries;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Queries;

public sealed class GetSessionStateQueryHandler(IVerificationSessionStore store)
    : IRequestHandler<GetSessionStateQuery, GetSessionStateResponse>
{
    public async Task<GetSessionStateResponse> Handle(GetSessionStateQuery request, CancellationToken ct)
    {
        var s = await store.GetAsync(request.SessionId, ct)
                ?? throw new NotFoundException("Сессия не найдена.");

        var available = (s.Phone, s.Email) switch
        {
            (not null, not null) => new[] { OtpChannel.phone, OtpChannel.email },
            (not null, null)     => new[] { OtpChannel.phone },
            _                    => new[] { OtpChannel.email }
        };

        var cooldown = s.NextResendAt.HasValue && s.NextResendAt > DateTime.UtcNow
            ? (int)(s.NextResendAt.Value - DateTime.UtcNow).TotalSeconds
            : 0;

        var ttl = s.ExpiresAt > DateTime.UtcNow
            ? (int)(s.ExpiresAt - DateTime.UtcNow).TotalSeconds
            : 0;

        return new GetSessionStateResponse(
            s.SessionId, available, s.Selected,
            s.Phone is null ? null : PhoneMasking.MaskPhoneLast4(s.Phone),
            s.Email,
            4, cooldown, ttl, s.AttemptsLeft
        );
    }
}