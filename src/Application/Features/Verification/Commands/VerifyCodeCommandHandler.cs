using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;
using MediatR;

namespace Application.Features.Verification.Commands;

public sealed class VerifyCodeCommandHandler(IVerificationSessionStore store)
    : IRequestHandler<VerifyCodeCommand, VerifyResponse>
{
    public async Task<VerifyResponse> Handle(VerifyCodeCommand request, CancellationToken ct)
    {
        var s = await store.GetAsync(request.SessionId, ct)
                ?? throw new NotFoundException("Сессия не найдена.");

        if (s.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Сессия истекла.");

        if (s.AttemptsLeft <= 0)
            throw new BadRequestException("Исчерпаны попытки.");

        if (!string.Equals(s.Code, request.Code, StringComparison.Ordinal))
        {
            var updated = s with { AttemptsLeft = s.AttemptsLeft - 1 };
            await store.UpdateAsync(updated, ct);
            throw new BadRequestException("Неверный код.");
        }

        if (s.Purpose == "reset")
        {
            // помечаем как подтверждённую «сессию сброса»
            var verified = s with { IsVerified = true, Code = string.Empty };
            await store.UpdateAsync(verified, ct);
        }
        else
        {
            // register / login → чистим
            await store.RemoveAsync(s.SessionId, ct);
        }

        return new VerifyResponse(true, "/");
    }
}