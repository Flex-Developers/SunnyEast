using Application.Contract.Order.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class OrderController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
}