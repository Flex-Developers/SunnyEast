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

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user = (await userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken))!;
            _ = user ?? throw new UnauthorizedAccessException("Почта не найдена.");
        }
        else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var normalized = NormalizePhoneOrNull(request.PhoneNumber) ?? request.PhoneNumber;
            user = (await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalized, cancellationToken))!;
            _ = user ?? throw new UnauthorizedAccessException("Телефонный номер не найден.");
        }
        else
        {
            throw new ValidationException("Ошибка, проверьте введенные данные!");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Неправильный пароль!");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserName!),
            new("uid", user.Id.ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        return new JwtTokenResponse
        {
            RefreshToken = jwtTokenService.CreateRefreshToken(claims),
            AccessToken = jwtTokenService.CreateTokenByClaims(claims, out var expireDate),
            AccessTokenValidateTo = expireDate
        };
    }

    private static string? NormalizePhoneOrNull(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var digits = System.Text.RegularExpressions.Regex.Replace(input, @"\D", "");
        if (digits.Length == 11 && (digits.StartsWith("8") || digits.StartsWith("7")))
            digits = "7" + digits[1..];
        else if (digits.Length == 10 && digits.StartsWith("9"))
            digits = "7" + digits;
        else if (!digits.StartsWith("7"))
            return null;

        return System.Text.RegularExpressions.Regex.Replace(
            digits, @"^7(\d{3})(\d{3})(\d{2})(\d{2})$", "+7-$1-$2-$3-$4");
    }
}