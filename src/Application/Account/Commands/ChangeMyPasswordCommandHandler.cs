using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Account.Commands;

public sealed class ChangeMyPasswordCommandHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        // Держим все операции в рамках UserManager (без db.SaveChanges)
        var user = await userManager.FindByNameAsync(userName)
                   ?? throw new NotFoundException("Пользователь не найден.");

        var res = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        res.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user);
        return Unit.Value;
    }
}