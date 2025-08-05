using Application.Contract.Verification.Responses;

namespace Application.Contract.Verification.Queries;

public sealed class CheckAvailabilityQuery : IRequest<CheckAvailabilityResponse>
{
    public string? Email { get; init; }
    public string? Phone { get; init; } // "+7-901-123-45-67"
}