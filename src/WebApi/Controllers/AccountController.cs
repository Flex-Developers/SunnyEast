using Application.Contract.Account.Commands;
using Application.Contract.Account.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers;

[Authorize]
[CustomExceptionsFilter] 
public sealed class AccountController : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var res = await Mediator.Send(new GetMyAccountQuery());
        return Ok(res);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent(); // 204
    }

    [HttpPut("email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }

    [HttpPut("phone")]
    public async Task<IActionResult> ChangePhone([FromBody] ChangePhoneCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }


    // Тихий рефреш токена после изменений профиля/email/телефона
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var token = await Mediator.Send(new GetMyJwtQuery());
        return Ok(token);
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        await Mediator.Send(new DeleteMyAccountCommand());
        return NoContent();
    }
    
    [HttpPost("link-contact")]
    public async Task<IActionResult> LinkContact([FromBody] LinkContactCommand cmd)
    {
        var result = await Mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPost("confirm-link")]
    public async Task<IActionResult> ConfirmLink([FromBody] ConfirmLinkCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }
}
