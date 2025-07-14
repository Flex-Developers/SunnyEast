using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Application.Contract.Enums;

namespace Application.Features.Orders.Queries;

public class GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Orders.AsQueryable();
        if (!string.IsNullOrEmpty(request.ShopSlug))
            query = query.Where(o => o.ShopSlug == request.ShopSlug);

        var orders = await query
            .Select(o => new OrderResponse
            {
                Slug = o.Slug,
                OrderNumber = o.OrderNumber,
                ShopSlug = o.ShopSlug!,
                Status = (OrderStatus)o.Status,
                Sum = o.OrderItems!.Sum(i => i.SummaryPrice)
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}