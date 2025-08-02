namespace Application.Contract.Staff.Commands;

public sealed class AssignStaffToShopCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string ShopSlug { get; set; } = default!;
}