using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Queries;
using Application.Contract.Staff.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Queries;

public sealed class GetStaffListQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetStaffListQuery, List<StaffResponse>>
{
    public async Task<List<StaffResponse>> Handle(GetStaffListQuery req, CancellationToken ct)
    {
        var staff = await context.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Shop)
            .ToListAsync(ct);

        return mapper.Map<List<StaffResponse>>(staff);
    }
}