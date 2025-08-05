using Application.Common.Interfaces.Contexts;
using Application.Contract.Verification.Queries;
using Application.Contract.Verification.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Verification.Queries;

public sealed class CheckAvailabilityQueryHandler(IApplicationDbContext db)
    : IRequestHandler<CheckAvailabilityQuery, CheckAvailabilityResponse>
{
    public async Task<CheckAvailabilityResponse> Handle(CheckAvailabilityQuery request, CancellationToken ct)
    {
        var phone = request.Phone;
        var email = request.Email;

        var phoneBusy = !string.IsNullOrWhiteSpace(phone) &&
                        await db.Users.AnyAsync(u => u.PhoneNumber == phone, ct);

        var emailBusy = !string.IsNullOrWhiteSpace(email) &&
                        await db.Users.AnyAsync(u => u.Email == email, ct);

        return new CheckAvailabilityResponse(
            PhoneAvailable: !phoneBusy,
            EmailAvailable: !emailBusy
        );
    }
}