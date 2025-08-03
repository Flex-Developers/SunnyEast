using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Common.Interfaces.Services;
using Application.Contract.User.Commands;
using Application.Contract.User.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands;

public class LoginUserCommandHandler(IJwtTokenService jwtTokenService, UserManager<ApplicationUser> userManager)
    : IRequestHandler<LoginUserCommand, JwtTokenResponse>
{
    public async Task<JwtTokenResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user;

        if (string.IsNullOrWhiteSpace(request.Email) == false)
        {
            user = (await userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken))!;
            _ = user ?? throw new UnauthorizedAccessException("Почта не найдена.");
        }
        else if (string.IsNullOrWhiteSpace(request.PhoneNumber) == false)
        {
            user = (await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber,
                cancellationToken))!;
            _ = user ?? throw new UnauthorizedAccessException("Телефонный номер не найден.");
        }
        else
            throw new ValidationException("Ошибка, проверьте введенные данные!");


        if (await userManager.CheckPasswordAsync(user, request.Password) == false)
            throw new UnauthorizedAccessException("Неправильный пароль!");

        if (!user.EmailConfirmed && !string.IsNullOrEmpty(user.Email))
            throw new UnauthorizedAccessException("Email не подтвержден.");


        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserName!), // как и было (для GetUserName())
            new("uid", user.Id.ToString())                  // НОВЫЙ клейм с GUID
        };

        var roles = await userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new JwtTokenResponse
        {
            RefreshToken = jwtTokenService.CreateRefreshToken(claims),
            AccessToken = jwtTokenService.CreateTokenByClaims(claims, out var expireDate),
            AccessTokenValidateTo = expireDate
        };
    }
}