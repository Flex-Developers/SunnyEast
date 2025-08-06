using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VerificationController : ApiControllerBase
{
    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] CheckAvailabilityQuery query)
        => Ok(await Mediator.Send(query));

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartVerificationCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("resend")]
    public async Task<IActionResult> Resend([FromBody] ResendCodeCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCodeCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetState([FromRoute] string sessionId)
        => Ok(await Mediator.Send(new GetSessionStateQuery { SessionId = sessionId }));
}