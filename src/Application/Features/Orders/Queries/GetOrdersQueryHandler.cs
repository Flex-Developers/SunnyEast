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
        var q = context.Orders
            .Include(o => o.Customer) // нужны данные клиента
            .Include(o => o.OrderItems) // чтобы корректно посчитать Sum
            .Include(o => o.Shop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ShopSlug))
            q = q.Where(o => o.ShopSlug == request.ShopSlug);

        return await q.ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}