using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed class GetAllOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetAllOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Shop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ShopSlug))
            orders = orders.Where(o => o.ShopSlug == request.ShopSlug);

        orders = request.OnlyArchived
            ? orders.Where(o => o.IsInArchive)
            : orders.Where(o => !o.IsInArchive);

        return await orders
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}