using System.Security.Claims;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
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
        var existingUser = await context.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.Phone || u.Email == request.Email,
            cancellationToken: cancellationToken);

        if (existingUser != null) // Check if user already registered
        {
            if (existingUser.Email == request.Email && existingUser.PhoneNumber == request.Phone)
                throw new BadRequestException($"Почта: {request.Email} и номер: {request.Phone} уже зарегистрированы!");

            if (existingUser.PhoneNumber == request.Phone)
                throw new BadRequestException($"Телефон: {request.Phone} уже зарегистрирован!");

            if (existingUser.Email == request.Email)
                throw new BadRequestException($"Почта: {request.Email} уже зарегистрирована!");

            if (await context.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
                throw new BadRequestException($"Имя пользователя: {request.UserName} не доступно!");
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
            Surname = request.Surname,
            Patronymic = request.Patronymic,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        result.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user);

        result = await userManager.AddClaimsAsync(user, [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Name, user.Name)
        ]);

        result.ThrowBadRequestIfError();
        await context.SaveChangesAsync(cancellationToken);

        return user.UserName;
    }
}