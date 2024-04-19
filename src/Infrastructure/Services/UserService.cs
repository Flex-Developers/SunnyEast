using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService(IApplicationDbContext context) : IUserService
{
    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Имя пользователя не может быть пустым!");
        
        return await context.Users.AnyAsync(u => u.UserName == username, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Эл-почта не может быть пустой");
        
        return await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            throw new ArgumentException("Тел-номер не может быть пустым");
        
        return await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }
}