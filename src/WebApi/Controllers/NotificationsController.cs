using Application.Contract.NotificationSubscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class NotificationsController(IConfiguration configuration) : ApiControllerBase
{
    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] CreateNotificationSubscriptionCommand subscription)
    {
        await Mediator.Send(subscription);
        return Ok();
    }

    [Authorize]
    [HttpDelete("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] DeleteNotificationSubscriptionCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }

    [Authorize]
    [HttpGet("public-key")]
    public IActionResult GetPublicKey()
    {
        var publicKey = configuration["VapidKeys:PublicKey"];
        return Ok(new { PublicKey = publicKey });
    }
}