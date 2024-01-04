using Application.Contract.Level.Commands;
using Application.Contract.Product.Commands;
using Application.Contract.Product.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class ProductController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> UpdateProduct(string slug, [FromBody] UpdateProductCommand command)
    {
        command.Slug = slug;
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteProduct(string slug)
    {
        await Mediator.Send(new DeleteLevelCommand
        {
            Slug = slug
        });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}