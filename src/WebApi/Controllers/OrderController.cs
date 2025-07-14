using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Identity;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class OrderController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var slug = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetOrder), new { slug }, new CreateOrderResponse(slug));
    }


    [HttpGet(nameof(GetOrder))]
    public async Task<IActionResult> GetOrder([FromQuery] GetOrderQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet(nameof(GetOrders))]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = ApplicationRoles.Salesman + "," + ApplicationRoles.Administrator)]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }
}
