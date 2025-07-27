using Application.Contract.Account.Commands;
using Application.Contract.Account.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers;

[Authorize]
[CustomExceptionsFilter] // уже используется в проекте
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

    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        await Mediator.Send(new LogoutAllCommand());
        return NoContent();
    }

    // Тихий рефреш токена после изменений профиля/email/телефона
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var token = await Mediator.Send(new GetMyJwtQuery());
        return Ok(token);
    }

    // (Опционально) Аватар — заглушка (реализацию хранения можно добавить позже)
    [HttpPost("avatar")]
    [DisableRequestSizeLimit]
    public IActionResult UploadAvatar()
    {
        // TODO: реализовать сохранение в wwwroot/avatars и записать путь в БД (поле AvatarUrl)
        return StatusCode(StatusCodes.Status501NotImplemented, "Загрузка аватара будет доступна позже.");
    }
}