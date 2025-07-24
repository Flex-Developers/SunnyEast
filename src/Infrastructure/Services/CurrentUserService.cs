using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? GetUserName()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetUserRole()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
    }
}