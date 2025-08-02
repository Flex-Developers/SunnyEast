using Application.Common.Interfaces.Contexts;
using Application.Contract.User.Queries;
using Application.Contract.User.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

public class GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetUserQuery, List<CustomerResponse>>
{
    public async Task<List<CustomerResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users.Where(s => true);
        if (request.UserName != null) 
            query = query.Where(s => s.UserName == request.UserName);

        if (request.Name != null) 
            query = query.Where(s => s.Name.Contains(request.Name));

        if (request.Phone != null) 
            query = query.Where(s => s.PhoneNumber != null && s.PhoneNumber.Contains(request.Phone));
        
        var users = await query.AsNoTracking()
            .OrderBy(u => u.Name).ThenBy(u => u.Surname)
            .ToListAsync(cancellationToken);

        // Get all staff user IDs
        var staffIds = await context.Staff
            .AsNoTracking()
            .Select(s => s.UserId)
            .ToListAsync(cancellationToken);
        
        var staffSet = staffIds.ToHashSet();

        // map + IsStaff
        var result = users.Select(u =>
        {
            var dto = mapper.Map<CustomerResponse>(u);
            dto.IsStaff = staffSet.Contains(u.Id);
            return dto;
        }).ToList();

        return result;
    }
}