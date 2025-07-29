using System.Security.Claims;
using Application.Contract.Order.Hub;
using Application.Contract.Order.Queries.Hub;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.Order;

[Authorize]
public sealed class OrderHub(IMediator mediator) : Hub<IOrderClient>
{
    /* ───── Короткие врапперы ───── */
    public static string ShopGroup(Guid id) => OrderGroupNames.Shop(id);
    public static string CustomerGroup(Guid id) => OrderGroupNames.Customer(id);
    public const string SuperAdminsGroup = OrderGroupNames.SuperAdminsGroup;

    public override async Task OnConnectedAsync()
    {
        var userName = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(userName))
        {
            await base.OnConnectedAsync();
            return;
        }

        var groups = await mediator.Send(
            new GetOrderHubGroupsQuery(userName, role ?? string.Empty),
            Context.ConnectionAborted);

        foreach (var g in groups)
            await Groups.AddToGroupAsync(Context.ConnectionId, g);

        await base.OnConnectedAsync();
    }
}