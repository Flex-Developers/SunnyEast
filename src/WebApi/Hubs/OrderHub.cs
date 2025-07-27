using System.Security.Claims;
using Application.Contract.Identity;
using Application.Contract.Order.Responses;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Hubs;


public interface IOrderClient
{
    Task OrderCreated(OrderResponse order);
    Task OrderStatusChanged(OrderResponse order);
    Task OrderArchived(OrderResponse order);
}

[Authorize]
public class OrderHub(ApplicationDbContext context) : Hub<IOrderClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var role   = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (userId is null)
            return;

        if (role == ApplicationRoles.SuperAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "superadmins");
        }
        else if (role is ApplicationRoles.Administrator or ApplicationRoles.Salesman)
        {
            var uid = Guid.Parse(userId);
            var shopIds = await context.Staff
                .Where(s => s.UserId == uid && s.IsActive && s.ShopId != null)
                .Select(s => s.ShopId!.Value)
                .Distinct()
                .ToListAsync();
            foreach (var id in shopIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"shop:{id}");
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer:{userId}");
        }

        await base.OnConnectedAsync();
    }
}
