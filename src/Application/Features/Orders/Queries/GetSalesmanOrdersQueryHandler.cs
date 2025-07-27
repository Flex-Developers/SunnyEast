using Application.Common.Exceptions;
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

public sealed class GetSalesmanOrdersQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<GetSalesmanOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetSalesmanOrdersQuery request, CancellationToken ct)
    {
        var userName = currentUser.GetUserName() ?? throw new UnauthorizedAccessException();
        var role = currentUser.GetUserRole();

        if (role is not (ApplicationRoles.Salesman or ApplicationRoles.Administrator or ApplicationRoles.SuperAdmin))
            throw new ForbiddenException();

        // Определяем список магазинов, доступных текущему пользователю
        IQueryable<Guid> myShopIds;

        if (role is ApplicationRoles.SuperAdmin)
        {
            myShopIds = context.Shops.Select(s => s.Id); // видит все
        }
        else
        {
            // Админ/Продавец — только магазины, где он в Staff и активен
            myShopIds = context.Staff
                .Where(s => s.User.UserName == userName && s.IsActive && s.ShopId != null)
                .Select(s => s.ShopId!.Value);
        }

        var orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Shop)
            .Where(o => myShopIds.Contains(o.ShopId));

        if (!string.IsNullOrWhiteSpace(request.ShopSlug))
            orders = orders.Where(o => o.ShopSlug == request.ShopSlug);

        orders = request.OnlyArchived ? orders.Where(o => o.IsInArchive) : orders.Where(o => !o.IsInArchive);

        return await orders
            .ProjectTo<OrderResponse>(mapper.ConfigurationProvider)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }
}