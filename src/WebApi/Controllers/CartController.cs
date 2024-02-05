using Application.Contract.Cart.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class CartController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteCart([FromBody] DeleteCartCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
}