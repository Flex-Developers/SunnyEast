using Application.Auth;
using Application.Contract.Auth;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers;

[CustomExceptionsFilter]
[Route("api/[controller]")]
public class AuthController(IEmailConfirmationSender emailConfirmationSender) : ControllerBase
{
    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        await emailConfirmationSender.ResendConfirmationAsync(request.Email, HttpContext.RequestAborted);
        return Accepted();
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult<ConfirmEmailResult>> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var success = await emailConfirmationSender.ConfirmEmailAsync(userId, token, HttpContext.RequestAborted);
        return Ok(new ConfirmEmailResult(success ? "success" : "invalid"));
    }
}
