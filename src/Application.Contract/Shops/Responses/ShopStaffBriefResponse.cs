using Application.Contract.Staff.Enums;

namespace Application.Contract.Shops.Responses;

public sealed class ShopStaffBriefResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty; 
    public StaffRole Role { get; set; }
}