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
        var created = await Mediator.Send(command);

        return CreatedAtAction(nameof(GetOrder), routeValues: new { slug = created.Slug }, value: created);
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

    [HttpPut("change-status")]
    [Authorize(Roles = ApplicationRoles.Salesman + "," + ApplicationRoles.Administrator)]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeOrderStatusCommand cmd)
    {
        await Mediator.Send(cmd);
        return NoContent();
    }

    [HttpPut("{slug}/cancel")]
    [Authorize] 
    public async Task<IActionResult> CancelOwnOrder(string slug)
    {
        await Mediator.Send(new CancelOrderCommand { Slug = slug });
        return NoContent();
    }



    [HttpPut("{slug}/archive")]
    public async Task<IActionResult> Archive([FromBody] ArchiveOrderCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}