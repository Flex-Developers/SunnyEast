using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;


internal class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string CreateTokenByClaims(IList<Claim> claims, out DateTime expireDate)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8
            .GetBytes(configuration["JWT:Secret"]!));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512Signature);
        expireDate = DateTime.Now.AddDays(int.Parse(configuration["JWT:TokenValidityInDays"]!));
        JwtSecurityToken token = new(
            claims: claims,
            expires: expireDate,
            signingCredentials: credentials);

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public string CreateRefreshToken(IList<Claim> claims)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8
            .GetBytes(configuration["JWT:Secret"]!));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512Signature);

        JwtSecurityToken token = new(
            claims: claims,
            expires: DateTime.Now.AddDays(
                int.Parse(configuration["JWT:RefreshTokenValidityInDays"]!)),
            signingCredentials: credentials);

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}