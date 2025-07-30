using MediatR;

namespace Application.Contract.NotificationSubscriptions;

public record SubscriptionKeys(string P256dh, string Auth);

public record CreateNotificationSubscriptionCommand(string Endpoint, SubscriptionKeys Keys) : IRequest<Unit>;