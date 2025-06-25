using Application.Contract.CartItem.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class CartItemController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCartItem([FromBody] CreateCartItemCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteCartItem(string slug)
    {
        var command = new DeleteCartItemCommand { Slug = slug };
        var response = await Mediator.Send(command);
        return Ok(response);
    }
}