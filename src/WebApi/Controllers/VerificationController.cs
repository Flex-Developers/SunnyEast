using Application.Contract.Verification.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/verify")]
public sealed class VerificationController(IMediator mediator) : ControllerBase
{
    [HttpPost("request-sms")]
    public async Task<IActionResult> RequestSms([FromBody] RequestSmsCommand cmd, CancellationToken ct)
        => Ok(await mediator.Send(cmd, ct));

    [HttpPost("confirm-registration")]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationCommand cmd, CancellationToken ct)
        => Ok(await mediator.Send(cmd, ct));

    [HttpPost("confirm-reset")]
    public async Task<IActionResult> ConfirmReset([FromBody] ConfirmResetPasswordCommand cmd, CancellationToken ct)
        => Ok(await mediator.Send(cmd, ct));
}