using System.Security.Claims;
using Application.Common.Interfaces.Contexts;
using Application.Contract.User.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands;

public class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext context)
    : IRequestHandler<RegisterUserCommand, string>
{
    public async Task<string> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var exitingUser = await context.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.Phone || u.Email == request.Email,
            cancellationToken: cancellationToken);
        if (exitingUser != null)
        {
            if (exitingUser.Email == request.Email && exitingUser.PhoneNumber == request.Phone)
            {
                throw new DuplicateWaitObjectException(
                    $"Email {request.Email} and Phone Number {request.Phone} are not available!");
            }

            if (exitingUser.PhoneNumber == request.Phone)
            {
                throw new DuplicateWaitObjectException($"Phone Number {request.Phone} is not available!");
            }

            if (exitingUser.Email == request.Email)
            {
                throw new DuplicateWaitObjectException($"Email {request.Email} is not available!");
            }
        }

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            NormalizedUserName = request.UserName.ToLower(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToLower(),
            EmailConfirmed = false,
            PhoneNumber = request.Phone,
            Name = request.Name,
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException(result.Errors.First().Description);
        }

        await context.SaveChangesAsync(cancellationToken);
        await CreateClaimAsync(user.UserName, user.Id, user.Email, cancellationToken);
        return user.UserName;
    }


    private async Task CreateClaimAsync(string username, Guid id, string email,
        CancellationToken cancellationToken = default)
    {
        var userClaims = new[]
        {
            new IdentityUserClaim<Guid>
            {
                UserId = id,
                ClaimType = ClaimTypes.NameIdentifier,
                ClaimValue = username,
            },
            new IdentityUserClaim<Guid>
            {
                UserId = id, ClaimType = ClaimTypes.Email, ClaimValue = email
            }
        };
        await context.UserClaims.AddRangeAsync(userClaims, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}