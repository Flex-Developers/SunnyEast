using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.User.Commands;
using Application.Contract.User.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class UserController(IUserService userService) : ApiControllerBase
{
    // [HttpGet("check-phone")]
    // public async Task<IActionResult> IsPhoneNumberExists(string phoneNumber)
    // {
    //     var result = await userService.IsPhoneNumberExistsAsync(phoneNumber, CancellationToken.None);
    //     return Ok(result);
    // }
    // [HttpGet("check-email")]
    // public async Task<IActionResult> CheckEmailExists(string email)
    // {
    //     var result = await userService.IsEmailExistsAsync(email, CancellationToken.None);
    //     return Ok(result);
    // }
    //
    // [HttpGet("check-username")]
    // public async Task<IActionResult> CheckUsernameExists(string username)
    // {
    //     var result = await userService.IsUsernameExistsAsync(username, CancellationToken.None);
    //     return Ok(result);
    // }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.Salesman + "," + ApplicationRoles.Administrator)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUserQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}