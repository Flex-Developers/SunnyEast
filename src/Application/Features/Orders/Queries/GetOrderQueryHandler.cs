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
    public async Task<OrderResponse> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await context.Orders
                        .Include(o => o.OrderItems!)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.Slug == request.Slug, ct)
                    ?? throw new NotFoundException($"Заказ {request.Slug} не найден.");

        var response = mapper.Map<OrderResponse>(order);

        response.Items = mapper.Map<List<OrderItemResponse>>(order.OrderItems!);
        response.Sum = response.Items.Sum(i => i.SummaryPrice);

        return response;
    }
}