using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public class GetOrderQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<GetOrderQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Shop)
                        .Include(o => o.OrderItems!).ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.Slug == request.Slug, ct)
                    ?? throw new NotFoundException($"Заказ {request.Slug} не найден.");

        var userName = currentUser.GetUserName() ?? throw new UnauthorizedAccessException();
        var role = currentUser.GetUserRole();

        var isSuperAdmin = role == ApplicationRoles.SuperAdmin;

        if (!isSuperAdmin)
        {
            var isStaff = role is ApplicationRoles.Administrator or ApplicationRoles.Salesman;
            if (isStaff)
            {
                var myShopIds = await context.Staff
                    .Where(s => s.User.UserName == userName && s.IsActive && s.ShopId != null)
                    .Select(s => s.ShopId!.Value)
                    .ToListAsync(ct);

                if (!myShopIds.Contains(order.ShopId))
                    throw new ForbiddenException(); // чужой магазин
            }
            else
            {
                // Покупатель — только свои заказы
                if (!string.Equals(order.Customer?.UserName, userName, StringComparison.OrdinalIgnoreCase))
                    throw new ForbiddenException();
            }
        }

        var response = mapper.Map<OrderResponse>(order);
        response.Items = mapper.Map<List<OrderItemResponse>>(order.OrderItems!);
        response.Sum = response.Items.Sum(i => i.SummaryPrice);
        return response;
    }
}