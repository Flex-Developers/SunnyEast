namespace Application.Contract.Staff.Commands;

public sealed class SetStaffActiveCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}