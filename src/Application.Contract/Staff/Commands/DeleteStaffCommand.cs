namespace Application.Contract.Staff.Commands;

public sealed class DeleteStaffCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}