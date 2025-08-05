using Application.Contract.Verification.Responses;

namespace Application.Contract.Verification.Commands;

public sealed class StartVerificationCommand : IRequest<StartVerificationResponse>
{
    public required string Purpose { get; init; } = "register";
    public string? UserName { get; init; }
    public string? Email    { get; init; }
    public string? Phone    { get; init; } // ожидаем "+7-901-123-45-67" (как в проекте)
}