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
    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            request.PhoneNumber = "+7-" + request.PhoneNumber;
        
        var existingUser = await context.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.PhoneNumber || u.Email == request.Email,
            cancellationToken: cancellationToken);

        if (existingUser != null) // Check if user already registered
        {
            if (existingUser.Email == request.Email && existingUser.PhoneNumber == request.PhoneNumber)
                throw new ExistException($"Почта: {request.Email} и номер: {request.PhoneNumber} уже зарегистрированы!\nВыполните вход.");

            if (existingUser.PhoneNumber == request.PhoneNumber)
                throw new ExistException($"Телефон: {request.PhoneNumber} уже зарегистрирован!\nВыполните вход.");

            if (existingUser.Email == request.Email)
                throw new ExistException($"Почта: {request.Email} уже зарегистрирована!\nВыполните вход.");
        }

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid(),
            Email = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : null,
            NormalizedEmail = request.Email!.ToLower(),
            EmailConfirmed = false,
            PhoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber) ? request.PhoneNumber : null,
            Name = request.Name,
            Surname = request.Surname,
            UserName = "User" + context.Users.Count()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        result.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user);

        result = await userManager.AddClaimsAsync(user, [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim("uid", user.Id.ToString()),           // НОВОЕ
            new Claim(ClaimTypes.Name, user.Name)
        ]);


        result.ThrowBadRequestIfError();
        await context.SaveChangesAsync(cancellationToken);

        return user.UserName!;
    }
}