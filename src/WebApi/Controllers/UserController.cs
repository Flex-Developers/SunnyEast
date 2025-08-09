using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.User.Commands;
using Application.Contract.User.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers;

[CustomExceptionsFilter]
public class UserController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUserQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
    
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteUserCommand { Id = id });
        return NoContent();
    }
}