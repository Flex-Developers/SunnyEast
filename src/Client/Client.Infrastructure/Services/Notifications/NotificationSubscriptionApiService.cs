using Application.Contract.NotificationSubscriptions;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Notifications;

public interface INotificationSubscriptionApiService
{
    Task<bool> CreateSubscriptionAsync(CreateNotificationSubscriptionCommand subscription);
    Task<bool> DeleteSubscriptionAsync(string unsubscribeEndpoint);
}

public class NotificationSubscriptionApiService(IHttpClientService httpClientService)
    : INotificationSubscriptionApiService
{
    public async Task<bool> CreateSubscriptionAsync(CreateNotificationSubscriptionCommand subscription)
    {
        try
        {
            var response = await httpClientService.PostAsJsonAsync("/api/notifications/subscribe", subscription);
            return response.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(string unsubscribeEndpoint)
    {
        try
        {
            var response =
                await httpClientService.DeleteAsync("/api/notifications/unsubscribe?endpoint=", unsubscribeEndpoint);
            return response.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }
}