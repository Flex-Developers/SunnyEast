using Application.Contract.Staff.Enums;
using Application.Contract.Staff.Responses;

namespace Client.Infrastructure.Services.Staff;

public interface IStaffService
{
    Task<List<StaffResponse>> GetAsync(string? shopSlug = null, StaffRole? role = null, bool? isActive = null, string? search = null);
    Task<StaffResponse?> GetByUserIdAsync(Guid userId);
    Task<bool> HireAsync(Guid userId);
    Task<bool> ChangeRoleAsync(Guid userId, StaffRole role);
    Task<bool> AssignAsync(Guid userId, string shopSlug);
    Task<bool> UnassignAsync(Guid userId);
    Task<bool> SetActiveAsync(Guid userId, bool isActive);
    Task<bool> DeleteAsync(Guid userId);
}