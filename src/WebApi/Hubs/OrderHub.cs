using System.Security.Claims;
using Application.Common.Interfaces.Contexts;
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
public sealed class OrderHub(IApplicationDbContext db) : Hub<IOrderClient>
{
    // ЕДИНЫЕ ключи групп — будем использовать и в хабе, и в нотифаере
    public static string ShopGroup(Guid shopId)         => $"shop:{shopId}";
    public const  string SuperAdminsGroup               = "superadmins";
    public static string CustomerGroup(Guid customerId) => $"customer:{customerId}";

    public override async Task OnConnectedAsync()
    {
        // 1) Узнаём роль и "имя пользователя" из токена (у вас в NameIdentifier лежит UserName)
        var userName = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role     = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(userName))
        {
            await base.OnConnectedAsync();
            return;
        }

        if (role == ApplicationRoles.SuperAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SuperAdminsGroup);
        }
        else if (role is ApplicationRoles.Administrator or ApplicationRoles.Salesman)
        {
            // 2) Для персонала определяем список магазинов по userName через Staff
            var shopIds = await db.Staff
                .Where(s => s.User.UserName == userName && s.IsActive && s.ShopId != null)
                .Select(s => s.ShopId!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var id in shopIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, ShopGroup(id));
        }
        else
        {
            // 3) Для покупателя переводим userName -> GUID Id и кладём в группу customer:{Id}
            var customerId = await db.Users
                .Where(u => u.UserName == userName)
                .Select(u => u.Id)
                .FirstAsync();

            await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(customerId));
        }

        await base.OnConnectedAsync();
    }
}
