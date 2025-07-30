using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.NotificationSubscriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Notifications.Commands;

public sealed class DeleteNotificationSubscriptionCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteNotificationSubscriptionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteNotificationSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        var subscription = await context.NotificationSubscriptions
            .FirstOrDefaultAsync(ns => ns.Endpoint == request.Endpoint && ns.UserId == userId, cancellationToken);

        if (subscription != null)
        {
            context.NotificationSubscriptions.Remove(subscription);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
