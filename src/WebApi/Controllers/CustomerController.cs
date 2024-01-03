using Application.Contract.Customer.Commands;
using Application.Contract.Customer.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class CustomerController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> UpdateCustomer(string slug, [FromBody] UpdateCustomerCommand command)
    {
        command.Slug = slug;
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteCustomer(string slug)
    {
        await Mediator.Send(new DeleteLevelCommand
        {
            Slug = slug
        });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}