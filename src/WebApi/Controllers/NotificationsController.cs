using Application.Contract.NotificationSubscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class NotificationsController : ApiControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateNotificationSubscriptionCommand subscription)
    {
        await Mediator.Send(subscription);
        return Ok();
    }
}