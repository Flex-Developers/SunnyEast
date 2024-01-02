using Application.Contract.Customer.Commands;
using Application.Contract.Customer.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class CustomerController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CreateCustomerCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer(UpdateCustomerCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteCustomer(DeleteCustomerCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers(GetCustomersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}