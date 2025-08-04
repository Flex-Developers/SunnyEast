using Application.Contract.User.Responses;

namespace Application.Contract.Verification.Commands;


public sealed class ConfirmRegistrationCommand : IRequest<JwtTokenResponse>
{
    public string Phone { get; init; } = default!;
    public string Code  { get; init; } = default!;

    // данные, которые нужны для создания пользователя:
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? FullName { get; init; }
}