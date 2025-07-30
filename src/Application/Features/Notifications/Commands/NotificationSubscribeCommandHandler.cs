using Application.Common.Interfaces.Contexts;
using Application.Contract.NotificationSubscriptions;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Notifications.Commands;

public class NotificationSubscribeCommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
    : IRequestHandler<CreateNotificationSubscriptionCommand, Unit>
{
    public async Task<Unit> Handle(CreateNotificationSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var notificationSubscription = mapper.Map<NotificationSubscription>(command);

        await applicationDbContext.NotificationSubscriptions.AddAsync(notificationSubscription, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}