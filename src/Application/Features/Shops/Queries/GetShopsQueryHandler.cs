using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Queries;

public class GetShopsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetShopsQuery, List<ShopResponse>>
{
    public async Task<List<ShopResponse>> Handle(GetShopsQuery request, CancellationToken cancellationToken)
    {
        return mapper.Map<List<ShopResponse>>(await context.Shops.ToListAsync(cancellationToken));
    }
}