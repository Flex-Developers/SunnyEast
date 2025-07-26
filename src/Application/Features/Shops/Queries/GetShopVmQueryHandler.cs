using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Queries;

public sealed class GetShopVmQueryHandler(IApplicationDbContext context, IMapper mapper) 
    : IRequestHandler<GetShopVmQuery, List<ShopVm>>
{
    public async Task<List<ShopVm>> Handle(GetShopVmQuery request, CancellationToken cancellationToken)
    {
        return (await context.Shops.ToListAsync(cancellationToken)).Select(mapper.Map<ShopVm>).ToList();
    }
}