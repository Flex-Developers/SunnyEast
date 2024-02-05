using Application.Contract.Cart.Commands;
using Application.Contract.Cart.Queries;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace WebApi;

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
    public async Task<IActionResult> GetCarts([FromQuery] GetCartQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}