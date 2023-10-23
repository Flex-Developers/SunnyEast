using Application.Contract.ProductCategory.Commands;
using Application.Contract.ProductCategory.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

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

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var command = new DeleteProductCategoryCommand { Id = id };
        await Mediator.Send(command);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetProductCategoriesQuery()));
    }
}