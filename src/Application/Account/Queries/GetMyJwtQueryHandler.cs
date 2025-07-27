using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Queries;
using Application.Contract.User.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Account.Queries;

public sealed class GetMyJwtQueryHandler(
    IApplicationDbContext db,
    IJwtTokenService jwt,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyJwtQuery, JwtTokenResponse>
{
    public async Task<JwtTokenResponse> Handle(GetMyJwtQuery request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        var claims = new List<Claim>
        {
            // В проекте NameIdentifier уже используется для UserName (сохраняем совместимость)
            new(ClaimTypes.NameIdentifier, user.UserName!),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            // При желании добавьте Email/Phone в клеймы позже
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new JwtTokenResponse
        {
            RefreshToken = jwt.CreateRefreshToken(claims),
            AccessToken = jwt.CreateTokenByClaims(claims, out var expires),
            AccessTokenValidateTo = expires
        };
    }
}