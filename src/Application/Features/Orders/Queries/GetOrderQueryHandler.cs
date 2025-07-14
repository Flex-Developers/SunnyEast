using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public class GetOrderQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetOrderQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"The order with slug {request.Slug} not found.");

        var response = mapper.Map<OrderResponse>(order);
        response.Sum = order.OrderItems?.Sum(i => i.SummaryPrice) ?? 0;
        return response;
    }
}
