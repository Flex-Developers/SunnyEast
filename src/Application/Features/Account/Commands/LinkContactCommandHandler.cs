using System.ComponentModel.DataAnnotations;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;
using Application.Common.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class LinkContactCommandHandler(
    ICurrentUserService current,
    IApplicationDbContext db,
    IMediator bus)
    : IRequestHandler<LinkContactCommand, StartVerificationResponse>
{
    public async Task<StartVerificationResponse> Handle(LinkContactCommand req, CancellationToken ct)
    {
        // 1) Текущий пользователь
        var userName = current.GetType().GetMethod("GetUserName")?.Invoke(current, null) as string
                       ?? (current.GetType().GetProperty("UserName")?.GetValue(current) as string)
                       ?? throw new UnauthorizedAccessException();

        var me = await db.Users.FirstAsync(u => u.UserName == userName, ct);

        // 2) Валидации + проверка уникальности
        if (string.Equals(req.Channel, "email", StringComparison.OrdinalIgnoreCase))
        {
            var email = (req.Value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("E-mail обязателен.");

            var exists = await db.Users.AsNoTracking()
                .AnyAsync(u => u.Id != me.Id &&
                               u.Email != null &&
                               u.Email.ToLower() == email.ToLower(), ct);
            if (exists)
                throw new ExistException("Этот e-mail уже используется.");

            // 3) Стартуем верификацию через шину и возвращаем ответ
            return await bus.Send(new StartVerificationCommand
            {
                Purpose = "link",
                Email = email
            }, ct);
        }
        else if (string.Equals(req.Channel, "phone", StringComparison.OrdinalIgnoreCase))
        {
            var phoneRaw = (req.Value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(phoneRaw))
                throw new ValidationException("Телефон обязателен.");

            // В БД телефон хранится в формате +7-XXX-XXX-XX-XX,
            // для корректной проверки приводим к E.164 и сравниваем по E.164.
            var exists = await db.Users.AsNoTracking()
                .AnyAsync(u => u.Id != me.Id &&
                               u.PhoneNumber != null &&
                               PhoneMasking.NormalizeE164(u.PhoneNumber) ==
                               PhoneMasking.NormalizeE164(phoneRaw), ct);
            if (exists)
                throw new ExistException("Этот телефон уже используется.");

            return await bus.Send(new StartVerificationCommand
            {
                Purpose = "link",
                Phone = phoneRaw
            }, ct);
        }

        throw new ValidationException("Поддерживаются только каналы 'email' или 'phone'.");
    }
}
