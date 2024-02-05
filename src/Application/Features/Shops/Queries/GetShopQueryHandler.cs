using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Queries;

public class GetShopQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetShopQuery, List<ShopResponse>>
{
    public async Task<List<ShopResponse>> Handle(GetShopQuery request, CancellationToken cancellationToken)
    {
        if (request.Address != null)
            return (await context.Shops.Where(s => s.Address.Contains(request.Address))
                .ToListAsync(cancellationToken)).Select(mapper.Map<ShopResponse>).ToList();

        return (await context.Shops.ToListAsync(cancellationToken)).Select(mapper.Map<ShopResponse>).ToList();
    }
}