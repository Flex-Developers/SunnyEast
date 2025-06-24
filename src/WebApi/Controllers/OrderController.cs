using Application.Contract.Order.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class OrderController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateCartItemCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateOrder([FromBody] UpdateCartItemCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteOrder(string slug)
    {
        var command = new DeleteCartItemCommand { Slug = slug };
        var response = await Mediator.Send(command);
        return Ok();
    }
}