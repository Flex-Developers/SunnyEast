// using Application.Common;
// using Application.Common.Exceptions;
// using Application.Common.Interfaces.Services;
// using Application.Contract.Account.Commands;
// using Domain.Entities;
// using MediatR;
// using Microsoft.AspNetCore.Identity;
//
// namespace Application.Features.Account.Commands;
//
// public sealed class LogoutAllCommandHandler(
//     UserManager<ApplicationUser> userManager,
//     ICurrentUserService currentUser)
//     : IRequestHandler<LogoutAllCommand, Unit>
// {
//     public async Task<Unit> Handle(LogoutAllCommand request, CancellationToken ct)
//     {
//         var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
//                        ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);
//
//         if (string.IsNullOrWhiteSpace(userName))
//             throw new UnauthorizedAccessException("Пользователь не распознан.");
//
//         var user = await userManager.FindByNameAsync(userName)
//                    ?? throw new NotFoundException("Пользователь не найден.");
//
//         var res = await userManager.UpdateSecurityStampAsync(user);
//         res.ThrowBadRequestIfError();
//
//         return Unit.Value;
//     }
// }