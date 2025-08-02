using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Queries;
using Application.Contract.Staff.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Queries;

public sealed class GetStaffByUserIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetStaffByUserIdQuery, StaffResponse?>
{
    public async Task<StaffResponse?> Handle(GetStaffByUserIdQuery req, CancellationToken ct)
    {
        var entity = await context.Staff
            .Include(s => s.User)
            .Include(s => s.Shop)
            .FirstOrDefaultAsync(s => s.UserId == req.UserId, ct);

        return entity is null ? null : mapper.Map<StaffResponse>(entity);
    }
}