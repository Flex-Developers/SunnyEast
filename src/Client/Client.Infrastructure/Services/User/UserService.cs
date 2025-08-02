using Application.Contract.User.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.User;

public sealed class UserService(IHttpClientService http) : IUserService
{
    public async Task<List<CustomerResponse>> GetAllUsersAsync()
    {
        var res = await http.GetFromJsonAsync<List<CustomerResponse>>("/api/user");
        return res.Success ? (res.Response ?? []) : [];
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        var res = await http.DeleteAsync($"/api/user/{id}");
        return res.Success;
    }
}