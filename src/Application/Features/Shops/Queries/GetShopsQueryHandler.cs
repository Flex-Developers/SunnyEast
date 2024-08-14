using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Queries;

public class GetShopsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetShopsQuery, List<Shop>>
{
    public async Task<List<Shop>> Handle(GetShopsQuery request, CancellationToken cancellationToken)
    {
        return await context.Shops.ToListAsync(cancellationToken);
    }
}