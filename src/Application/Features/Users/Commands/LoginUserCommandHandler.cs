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
    public async Task<JwtTokenResponse> Handle(LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
        _ = user ?? throw new UnauthorizedAccessException("Username is not found.");

        var success = await userManager.CheckPasswordAsync(user, request.Password);

        if (!success)
        {
            throw new UnauthorizedAccessException("Incorrect password.");
        }

        var claims = await userManager.GetClaimsAsync(user);
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