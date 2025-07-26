using Application.Contract.Identity;
using Application.Contract.ProductCategory.Commands;
using Application.Contract.ProductCategory.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.SuperAdmin + "," + ApplicationRoles.Administrator)]
public class ProductCategoryController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProductCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{slug}")]
    public async Task<IActionResult> Delete(string slug)
    {
        var command = new DeleteProductCategoryCommand { Slug = slug };
        await Mediator.Send(command);
        return Ok();
    }

    [HttpGet("GetByName/{name}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByName([FromRoute]string name)
    {
        var result = await Mediator.Send(new GetProductCategoryQuery { Name = name });
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetProductCategoriesQuery()));
    }
}