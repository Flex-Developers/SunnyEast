using Application.Contract.Level.Commands;
using Application.Contract.Level.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class LevelController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateLevel([FromBody] CreateLevelCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> EditLevel(string slug, [FromBody] UpdateLevelCommand request)
    {
        await Mediator.Send(request);
        return Ok();
    }

    [HttpDelete("{slug}")]
    public async Task<IActionResult> DeleteLevel(string slug)
    {
        await Mediator.Send(new DeleteLevelCommand
        {
            Slug = slug
        });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels([FromQuery] GetLevelsQuery request)
    {
        var responses = await Mediator.Send(request);
        return Ok(responses);
    }
}