using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.NotificationSubscriptions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Notifications.Commands;

public sealed class CreateNotificationSubscriptionCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateNotificationSubscriptionCommand, Unit>
{
    public async Task<Unit> Handle(CreateNotificationSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        var existingSubscription = await context.NotificationSubscriptions
            .FirstOrDefaultAsync(ns => ns.UserId == userId, cancellationToken);

        if (existingSubscription != null)
        {
            existingSubscription.P256Dh = request.Keys.P256dh;
            existingSubscription.Auth = request.Keys.Auth;
            existingSubscription.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            var subscription = new NotificationSubscription
            {
                Endpoint = request.Endpoint,
                P256Dh = request.Keys.P256dh,
                Auth = request.Keys.Auth,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.NotificationSubscriptions.Add(subscription);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}