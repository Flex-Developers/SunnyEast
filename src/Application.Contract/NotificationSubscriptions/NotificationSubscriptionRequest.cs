namespace Application.Contract.NotificationSubscriptions;

public record SubscriptionKeys(string P256dh, string Auth);

public record NotificationSubscriptionRequest(string Endpoint, SubscriptionKeys Keys);

public record NotificationRequest(string Title, string Body);