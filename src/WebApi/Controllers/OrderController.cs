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
    
    [HttpGet("by-user/{userId:guid}")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    public async Task<ActionResult<List<OrderResponse>>> GetByUser(Guid userId)
    {
        var list = await Mediator.Send(new GetOrdersByUserQuery { UserId = userId });
        return Ok(list);
    }
    
    [HttpGet(nameof(GetAllOrders))]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllOrders([FromQuery] GetAllOrdersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
    
    [Authorize]
    [HttpGet("customer")]
    public async Task<IActionResult> GetCustomerOrders([FromQuery] GetCustomerOrdersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.AllStaffCsv)]
    [HttpGet("salesman")]
    public async Task<IActionResult> GetSalesmanOrders([FromQuery] GetSalesmanOrdersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpPut("change-status")]
    [Authorize(Roles = ApplicationRoles.AllStaffCsv)]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeOrderStatusCommand command)
    {
        await Mediator.Send(command);
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
    [Authorize(Roles = ApplicationRoles.AllStaffCsv)]
    public async Task<IActionResult> Archive([FromBody] ArchiveOrderCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}