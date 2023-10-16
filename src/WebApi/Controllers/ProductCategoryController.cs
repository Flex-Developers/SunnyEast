using Application.Contract.ProductCategory.Commands;
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
}