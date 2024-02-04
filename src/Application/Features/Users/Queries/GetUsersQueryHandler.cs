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
        if (request.UserName != null) query = query.Where(s => s.UserName == request.UserName);

        if (request.Name != null) query = query.Where(s => s.Name.Contains(request.Name));

        if (request.Phone != null) query = query.Where(s => s.Phone != null && s.Phone.Contains(request.Phone));


        return (await query.ToArrayAsync(cancellationToken))
            .Select(mapper.Map<CustomerResponse>).ToList();
    }
}