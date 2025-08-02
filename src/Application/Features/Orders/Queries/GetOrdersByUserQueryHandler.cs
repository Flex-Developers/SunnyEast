using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed class GetOrdersByUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetOrdersByUserQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}