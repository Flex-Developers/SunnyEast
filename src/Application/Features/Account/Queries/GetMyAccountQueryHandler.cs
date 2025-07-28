using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Queries;
using Application.Contract.Account.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Queries;

public sealed class GetMyAccountQueryHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyAccountQuery, MyAccountResponse>
{
    public async Task<MyAccountResponse> Handle(GetMyAccountQuery request, CancellationToken ct)
    {
        var userName = TryGetUserName();
        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var user = await db.Users.AsNoTracking()
                       .FirstOrDefaultAsync(u => u.UserName == userName, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        var roles = await userManager.GetRolesAsync(user);

        return new MyAccountResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Roles = roles.ToArray(),
            CreatedAt = user.CreatedAt,
            AvatarUrl = null // Заглушка
        };

        string? TryGetUserName()
        {
            try
            {
                // Поддержка и метода, и свойства
                var type = currentUser.GetType();
                var m = type.GetMethod("GetUserName");
                if (m != null) return m.Invoke(currentUser, null) as string;

                var p = type.GetProperty("UserName");
                if (p != null) return p.GetValue(currentUser) as string;
            }
            catch { /* ignored */ }
            return null;
        }
    }
}