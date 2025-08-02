using Application.Contract.Identity;
using Application.Contract.Product.Commands;
using Application.Contract.Product.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.SuperAdmin + "," + ApplicationRoles.Administrator)]
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
        await Mediator.Send(new DeleteProductCommand
        {
            Slug = slug
        });
        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet("GetProductsByCategoryName/{categoryName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsByCategoryName([FromRoute]string categoryName)
    {
        var response = await Mediator.Send(new GetProductsByCategoryNameQuery { CategoryName = categoryName });
        return Ok(response);
    }
}