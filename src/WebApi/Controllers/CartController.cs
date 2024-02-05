using Application.Contract.Cart.Commands;
using Application.Contract.Cart.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Controllers;

namespace WebApi;

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

    [HttpGet(nameof(GetCart))]
    public async Task<IActionResult> GetCart([FromQuery] GetCartQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet(nameof(GetCarts))]
    public async Task<IActionResult> GetCarts([FromQuery] GetCartsQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
    
}