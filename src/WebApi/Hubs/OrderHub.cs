using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Hubs;

[Authorize]
public sealed class OrderHub : Hub<IOrderClient>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public OrderHub(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public static string ShopGroup(Guid shopId) => $"shop:{shopId}";
    public const string SuperAdminsGroup = "superadmins";
    public static string CustomerGroup(Guid userId) => $"customer:{userId}";

    public override async Task OnConnectedAsync()
    {
        var role = _current.GetUserRole();
        var userName = _current.GetUserName() ?? throw new UnauthorizedAccessException();

        if (role == ApplicationRoles.SuperAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SuperAdminsGroup);
        }
        else if (role is ApplicationRoles.Administrator or ApplicationRoles.Salesman)
        {
            var shopIds = await _db.Staff
                .Where(s => s.User.UserName == userName && s.IsActive && s.ShopId != null)
                .Select(s => s.ShopId!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var id in shopIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, ShopGroup(id));
        }
        else
        {
            var customerId = await _db.Users
                .Where(u => u.UserName == userName)
                .Select(u => u.Id)
                .FirstAsync();

            await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(customerId));
        }

        await base.OnConnectedAsync();
    }
}
