namespace Application.Contract.Staff.Commands;

public sealed class HireUserAsStaffCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}