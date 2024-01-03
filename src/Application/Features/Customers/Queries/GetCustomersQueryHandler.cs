using Application.Common.Interfaces.Contexts;
using Application.Contract.Customer.Queries;
using Application.Contract.Customer.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class GetCustomersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetCustomersQuery, List<CustomerResponse>>
{
    public async Task<List<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Customers.Where(s => true);
        if (request.Slug != null) query = query.Where(s => s.Slug.Contains(request.Slug));

        if (request.Name != null) query = query.Where(s => s.Name.Contains(request.Name));

        if (request.Phone != null) query = query.Where(s => s.Phone != null && s.Phone.Contains(request.Phone));

        return (await query.ToArrayAsync(cancellationToken))
            .Select(mapper.Map<CustomerResponse>).ToList();
    }
}