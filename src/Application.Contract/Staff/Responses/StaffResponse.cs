using Application.Contract.Staff.Enums;

namespace Application.Contract.Staff.Responses;

public sealed class StaffResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Name { get; set; } = default!;
    public string? Surname { get; set; }

    public StaffRole Role { get; set; }
    public bool IsActive { get; set; }

    public Guid? ShopId { get; set; }
    public string? ShopSlug { get; set; }
    public string? ShopAddress { get; set; }
}