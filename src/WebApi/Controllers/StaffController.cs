using Application.Contract.Identity;
using Application.Contract.Staff.Commands;
using Application.Contract.Staff.Queries;
using Application.Contract.Staff.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.SuperAdmin)]
public sealed class StaffController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = ApplicationRoles.SuperAdmin + "," + ApplicationRoles.Administrator)]
    public async Task<ActionResult<List<StaffResponse>>> Get([FromQuery] GetStaffListQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin + "," + ApplicationRoles.Administrator)]
    public async Task<ActionResult<StaffResponse?>> GetByUserId(Guid userId)
    {
        return Ok(await Mediator.Send(new GetStaffByUserIdQuery { UserId = userId }));
    }
    
    [HttpPost("hire")]
    public async Task<IActionResult> Hire([FromBody] HireUserAsStaffCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPut("role")]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeUserRoleCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPut("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignStaffToShopCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{userId:guid}/assign")]
    public async Task<IActionResult> Unassign(Guid userId)
    {
        await Mediator.Send(new UnassignStaffFromShopCommand { UserId = userId });
        return NoContent();
    }

    [HttpPut("active")]
    public async Task<IActionResult> SetActive([FromBody] SetStaffActiveCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        await Mediator.Send(new DeleteStaffCommand { UserId = userId });
        return NoContent();
    }
}