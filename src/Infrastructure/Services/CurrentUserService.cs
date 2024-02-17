using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string GetUserName()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.Name!;
    }

    public int GetUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "UserId");
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            return userId;
        
        throw new NotFoundException("User ID claim not found or not valid");
    }
}