using Application.Contract.NotificationSubscriptions;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Notifications;

public interface INotificationSubscriptionApiService
{
    Task<bool> CreateSubscriptionAsync(NotificationSubscription subscription);
    Task<bool> DeleteSubscriptionAsync(string endpoint);
    Task<bool> UpdateSubscriptionAsync(NotificationSubscription subscription);
}

public class NotificationSubscriptionApiService(IHttpClientService httpClientService)
    : INotificationSubscriptionApiService
{
    public async Task<bool> CreateSubscriptionAsync(NotificationSubscription subscription)
    {
        try
        {
            var command = new CreateNotificationSubscriptionCommand(
                subscription.Endpoint,
                new SubscriptionKeys(subscription.Keys.P256dh, subscription.Keys.Auth)
            );

            var response = await httpClientService.PostAsJsonAsync("api/notifications/subscribe", command);
            return response.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(string endpoint)
    {
        try
        {
            var response = await httpClientService.DeleteAsync($"api/notifications/unsubscribe?endpoint={Uri.EscapeDataString(endpoint)}");
            return response.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(NotificationSubscription subscription)
    {
        try
        {
            // For updates, we can delete the old one and create a new one
            // Or implement a dedicated update endpoint if needed
            return await CreateSubscriptionAsync(subscription);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
