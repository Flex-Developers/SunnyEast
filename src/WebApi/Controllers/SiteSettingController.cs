using Application.Contract.Identity;
using Application.Contract.SiteSettings.Commands;
using Application.Contract.SiteSettings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class SiteSettingController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSetting()
    {
        var response = await Mediator.Send(new GetSiteSettingQuery());
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSiteSettingCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }
}
