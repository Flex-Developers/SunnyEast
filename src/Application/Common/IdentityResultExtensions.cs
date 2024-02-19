using Application.Common.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Application.Common;

public static class IdentityResultExtensions
{
    public static void ThrowBadRequestIfError(this IdentityResult result)
    {
        result.ThrowIfError(typeof(BadRequestException));
    }
    
    public static void ThrowInvalidOperationIfError(this IdentityResult result)
    {
        result.ThrowIfError(typeof(BadRequestException));
    }

    private static void ThrowIfError(this IdentityResult result, Type exceptionType)
    {
        if (!result.Succeeded)
        {
            Activator.CreateInstance(exceptionType, result.Errors.First().Description);
        }
    }
}