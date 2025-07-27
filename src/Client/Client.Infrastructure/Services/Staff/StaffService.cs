using Application.Contract.Staff.Commands;
using Application.Contract.Staff.Enums;
using Application.Contract.Staff.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Staff;

public sealed class StaffService(IHttpClientService http) : IStaffService
{
    public async Task<List<StaffResponse>> GetAsync(string? shopSlug = null, StaffRole? role = null, bool? isActive = null, string? search = null)
    {
        var url = "/api/staff?";
        
        if (!string.IsNullOrWhiteSpace(shopSlug)) 
            url += $"ShopSlug={Uri.EscapeDataString(shopSlug)}&";
        
        if (role.HasValue) 
            url += $"Role={(int)role.Value}&";
        
        if (isActive.HasValue) 
            url += $"IsActive={isActive.Value.ToString().ToLower()}&";
        
        if (!string.IsNullOrWhiteSpace(search)) 
            url += $"Search={Uri.EscapeDataString(search)}&";

        var res = await http.GetFromJsonAsync<List<StaffResponse>>(url);
        return res.Success ? res.Response ?? [] : [];
    }
    
    public async Task<bool> HireAsync(Guid userId)
    {
        var res = await http.PostAsJsonAsync($"/api/staff/hire", new { UserId = userId });
        return res.Success;
    }

    public async Task<StaffResponse?> GetByUserIdAsync(Guid userId)
    {
        var res = await http.GetFromJsonAsync<StaffResponse>($"/api/staff/{userId}");
        return res.Success ? res.Response : null;
    }

    public async Task<bool> ChangeRoleAsync(Guid userId, StaffRole role)
    {
        var cmd = new ChangeUserRoleCommand { UserId = userId, Role = role };
        var res = await http.PutAsJsonAsync("/api/staff/role", cmd);
        return res.Success;
    }

    public async Task<bool> AssignAsync(Guid userId, string shopSlug)
    {
        var cmd = new AssignStaffToShopCommand { UserId = userId, ShopSlug = shopSlug };
        var res = await http.PutAsJsonAsync("/api/staff/assign", cmd);
        return res.Success;
    }

    public async Task<bool> UnassignAsync(Guid userId)
    {
        var res = await http.DeleteAsync($"/api/staff/{userId}/assign");
        return res.Success;
    }

    public async Task<bool> SetActiveAsync(Guid userId, bool isActive)
    {
        var cmd = new SetStaffActiveCommand { UserId = userId, IsActive = isActive };
        var res = await http.PutAsJsonAsync("/api/staff/active", cmd);
        return res.Success;
    }
    
    public async Task<bool> DeleteAsync(Guid userId)
    {
        var res = await http.DeleteAsync($"/api/staff/{userId}");
        return res.Success;
    }
}
