using MediatR;

namespace Application.Contract.NotificationSubscriptions;

public record DeleteNotificationSubscriptionCommand(string Endpoint) : IRequest<Unit>;
