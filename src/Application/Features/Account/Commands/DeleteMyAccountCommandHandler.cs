using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Application.Contract.Identity;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands;

public sealed class DeleteMyAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteMyAccountCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMyAccountCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var user = await userManager.FindByNameAsync(userName)
                   ?? throw new NotFoundException("Пользователь не найден.");
        
        if (await userManager.IsInRoleAsync(user, ApplicationRoles.SuperAdmin))
            throw new ForbiddenException("Нельзя удалить аккаунт супер-администратора.");

        var res = await userManager.DeleteAsync(user);
        res.ThrowBadRequestIfError();

        return Unit.Value;
    }
}