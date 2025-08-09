using System.ComponentModel.DataAnnotations;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Application.Contract.Verification.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class LinkContactCommandHandler(
    ICurrentUserService current,
    IApplicationDbContext db,
    IMediator bus)
    : IRequestHandler<LinkContactCommand, Unit>
{
    public async Task<Unit> Handle(LinkContactCommand req, CancellationToken ct)
    {
        var userName = current.GetType().GetMethod("GetUserName")?.Invoke(current, null) as string
                       ?? (current.GetType().GetProperty("UserName")?.GetValue(current) as string)
                       ?? throw new UnauthorizedAccessException();

        var user = await db.Users.FirstAsync(u => u.UserName == userName, ct);

        if (req.Channel == "email")
        {
            if (string.IsNullOrWhiteSpace(req.Value))
                throw new ValidationException("E-mail обязателен.");

            var exists = await db.Users.AnyAsync(u => u.Email != null && u.Email == req.Value, ct);
            if (exists) throw new ExistException("Этот e-mail уже используется.");

            // стартуем верификацию
            await bus.Send(new StartVerificationCommand
            {
                Purpose = "link",
                Email = req.Value
            }, ct);
        }
        else /* phone */
        {
            if (string.IsNullOrWhiteSpace(req.Value))
                throw new ValidationException("Телефон обязателен.");

            var exists = await db.Users.AnyAsync(u => u.PhoneNumber == req.Value, ct);
            if (exists) throw new ExistException("Этот телефон уже используется.");

            await bus.Send(new StartVerificationCommand
            {
                Purpose = "link",
                Phone = req.Value
            }, ct);
        }

        return Unit.Value;
    }
}