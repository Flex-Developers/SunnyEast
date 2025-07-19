using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed class GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = context.Orders
            .Include(o => o.Customer) // нужны данные клиента
            .Include(o => o.OrderItems) // чтобы корректно посчитать Sum
            .Include(o => o.Shop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ShopSlug))
            orders = orders.Where(o => o.ShopSlug == request.ShopSlug);
        
        orders = request.OnlyArchived ? orders.Where(o => o.IsInArchive) : orders.Where(o => !o.IsInArchive);


        return await orders.ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}