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
        // 1) Нормализуем телефон в канонический вид или делаем null
        request.PhoneNumber = NormalizePhoneOrNull(request.PhoneNumber);

        // 2) Проверка занятости (как было)
        var existingUser = await context.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == request.PhoneNumber || u.Email == request.Email,
            cancellationToken);

        if (existingUser != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Email) && existingUser.Email == request.Email
                                                          && !string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                                                          existingUser.PhoneNumber == request.PhoneNumber)
                throw new ExistException(
                    $"Почта: {request.Email} и номер: {request.PhoneNumber} уже зарегистрированы!\nВыполните вход.");

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && existingUser.PhoneNumber == request.PhoneNumber)
                throw new ExistException($"Телефон: {request.PhoneNumber} уже зарегистрирован!\nВыполните вход.");

            if (!string.IsNullOrWhiteSpace(request.Email) && existingUser.Email == request.Email)
                throw new ExistException($"Почта: {request.Email} уже зарегистрирована!\nВыполните вход.");
        }

        // 3) Создаём пользователя
        ApplicationUser user = new()
        {
            Id = Guid.NewGuid(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
            NormalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.ToLower(),
            EmailConfirmed = false,
            PhoneNumber = request.PhoneNumber, // уже нормализован или null
            Name = request.Name,
            Surname = request.Surname,
            UserName = request.Name + request.Surname + context.Users.Count()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        result.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user);

        result = await userManager.AddClaimsAsync(user, [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim("uid", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        ]);
        result.ThrowBadRequestIfError();

        await context.SaveChangesAsync(cancellationToken);
        return user.UserName!;
    }

    private static string? NormalizePhoneOrNull(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var digits = System.Text.RegularExpressions.Regex.Replace(input, @"\D", ""); // только цифры
        if (digits.Length == 11 && (digits.StartsWith("8") || digits.StartsWith("7")))
            digits = "7" + digits[1..];
        else if (digits.Length == 10 && digits.StartsWith("9"))
            digits = "7" + digits;
        else if (!digits.StartsWith("7")) // всё остальное считаем невалидным для нашего кейса
            return null;

        return System.Text.RegularExpressions.Regex.Replace(
            digits, @"^7(\d{3})(\d{3})(\d{2})(\d{2})$", "+7-$1-$2-$3-$4");
    }
}