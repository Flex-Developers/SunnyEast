using Application.Contract.Identity;
using Application.Contract.Order.Responses;
using Application.Contract.Realtime;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Hubs;

[Authorize]
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
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            await base.OnConnectedAsync();
            return;
        }

        var userIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (Guid.TryParse(userIdStr, out var userId))
        {
            if (role == ApplicationRoles.SuperAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "superadmins");
            }
            else if (role is ApplicationRoles.Administrator or ApplicationRoles.Salesman)
            {
                var shopIds = await context.Staff
                    .Where(s => s.UserId == userId && s.IsActive && s.ShopId != null)
                    .Select(s => s.ShopId!.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in shopIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"shop:{id}");
                }
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer:{userId}");
            }
        }

        await base.OnConnectedAsync();
    }
}
