using System.Security.Claims;
using Application.Common;
using Application.Common.Exceptions;
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
        var existingUser = await context.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.Phone || u.Email == request.Email,
            cancellationToken: cancellationToken);
        if (existingUser != null)
        {
            if (existingUser.Email == request.Email && existingUser.PhoneNumber == request.Phone)
            {
                throw new BadRequestException(
                    $"Email {request.Email} and Phone Number {request.Phone} are not available!");
            }

            if (existingUser.PhoneNumber == request.Phone)
            {
                throw new BadRequestException($"Phone Number {request.Phone} is not available!");
            }

            if (existingUser.Email == request.Email)
            {
                throw new BadRequestException($"Email {request.Email} is not available!");
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
        result.ThrowBadRequestIfError();

        result = await userManager.AddClaimsAsync(user, [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Name, user.Name)
        ]);
        result.ThrowBadRequestIfError();

        return user.UserName;
    }
}