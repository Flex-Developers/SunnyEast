using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed class GetCustomerOrdersQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
    : IRequestHandler<GetCustomerOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var userName = currentUser.GetUserName();

        var orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Shop)
            .Where(o => o.Customer!.UserName == userName);

        return await orders
            .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}