using Application.Contract.Shops.Commands;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class ShopController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteShop([FromBody] DeleteShopCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateShop([FromBody] UpdateShopCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
}