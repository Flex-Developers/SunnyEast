using Application.Contract.Staff.Enums;

namespace Application.Contract.Staff.Commands;

public sealed class ChangeUserRoleCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public StaffRole Role { get; set; } = StaffRole.None;
}