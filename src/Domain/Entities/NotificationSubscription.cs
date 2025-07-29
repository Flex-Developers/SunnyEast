namespace Domain.Entities;

#nullable disable
public class NotificationSubscription : BaseEntity
{
    public string Endpoint { get; set; }
    public string P256Dh { get; set; }
    public string Auth { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
}