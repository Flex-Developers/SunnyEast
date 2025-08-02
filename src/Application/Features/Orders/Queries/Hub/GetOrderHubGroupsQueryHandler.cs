using Application.Common.Interfaces.Contexts;
using Application.Contract.Identity;
using Application.Contract.Order.Hub;
using Application.Contract.Order.Queries.Hub;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.Hub;

public sealed class GetOrderHubGroupsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetOrderHubGroupsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetOrderHubGroupsQuery request,
        CancellationToken ct)
    {
        var groups = new List<string>();

        if (request.Role == ApplicationRoles.SuperAdmin)
        {
            groups.Add(OrderGroupNames.SuperAdminsGroup);
            return groups;
        }

        if (request.Role is ApplicationRoles.Salesman or ApplicationRoles.Administrator)
        {
            var shopIds = await db.Staff
                .Where(s => s.User.UserName == request.UserName
                            && s.IsActive
                            && s.ShopId != null)
                .Select(s => s.ShopId!.Value)
                .Distinct()
                .ToListAsync(ct);

            foreach (var id in shopIds)
                groups.Add(OrderGroupNames.Shop(id));

            return groups;
        }

        // Покупатель
        var customerId = await db.Users
            .Where(u => u.UserName == request.UserName)
            .Select(u => u.Id)
            .FirstAsync(ct);

        groups.Add(OrderGroupNames.Customer(customerId));
        return groups;
    }
}