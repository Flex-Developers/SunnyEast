namespace Application.Common.Interfaces.Services;

public interface IPushNotificationService
{
    Task PushAsync(Guid userId, string actionType, string header, string body);
}