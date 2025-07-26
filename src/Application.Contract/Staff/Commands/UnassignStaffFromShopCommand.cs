namespace Application.Contract.Staff.Commands;

public sealed class UnassignStaffFromShopCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}