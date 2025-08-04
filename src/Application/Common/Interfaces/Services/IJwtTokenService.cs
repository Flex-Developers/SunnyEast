using System.Security.Claims;
using Application.Contract.User.Responses;
using Domain.Entities;

namespace Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    public string CreateTokenByClaims(IList<Claim> user, out DateTime expireDate);
    public string CreateRefreshToken(IList<Claim> user);
    public Task<JwtTokenResponse> GenerateAsync(ApplicationUser user);
}