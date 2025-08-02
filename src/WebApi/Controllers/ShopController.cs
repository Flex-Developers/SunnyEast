using Application.Contract.Identity;
using Application.Contract.Shops.Commands;
using Application.Contract.Shops.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.SuperAdmin)]
public class ShopController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteShop(string slug)
    {
        await Mediator.Send(new DeleteShopCommand { Slug = slug });
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateShop([FromBody] UpdateShopCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    
    [HttpGet(nameof(GetShopVm))]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopVm([FromQuery] GetShopVmQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet(nameof(GetShop))]
    [AllowAnonymous]
    public async Task<IActionResult> GetShop([FromQuery] GetShopQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet(nameof(GetShops))]
    [AllowAnonymous]
    public async Task<IActionResult> GetShops([FromQuery] GetShopsQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}