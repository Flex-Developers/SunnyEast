using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Identity;
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
