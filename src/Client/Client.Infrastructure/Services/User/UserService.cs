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
}