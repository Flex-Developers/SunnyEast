using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed class GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        //TODO: Fix current user detecting!
        var userId = currentUserService.GetUserId();
        
        if (string.IsNullOrEmpty(userId.ToString()))
            return [];   
        
        var orders = context.Orders
            .Include(o => o.Customer) // нужны данные клиента
            .Include(o => o.OrderItems) // чтобы корректно посчитать Sum
            .Include(o => o.Shop)
            .Where(o => o.CustomerId == userId) // только для текущего пользователя
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ShopSlug))
            orders = orders.Where(o => o.ShopSlug == request.ShopSlug);
        
        orders = request.OnlyArchived ? orders.Where(o => o.IsInArchive) : orders.Where(o => !o.IsInArchive);

        return await orders.ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}