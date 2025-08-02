using Application.Contract.User.Responses;

namespace Client.Infrastructure.Services.User;

public interface IUserService
{
    Task<List<CustomerResponse>> GetAllUsersAsync();
    Task<bool> DeleteAsync(Guid id);
}