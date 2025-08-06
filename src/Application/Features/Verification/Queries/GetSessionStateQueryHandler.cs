using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
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
            s.Phone is null ? null : MaskPhone(s.Phone),
            s.Email,
            4, cooldown, ttl, s.AttemptsLeft
        );
    }

    private static string MaskPhone(string phone) // "+7-901-123-45-67" -> "+7-***-***-45-67"
    {
        // Предполагаем формат "+7-XXX-XXX-XX-XX"
        var last2a = phone[^4..^2]; // "45"
        var last2b = phone[^2..];   // "67"
        return $"+7-***-***-{last2a}-{last2b}";
    }
}