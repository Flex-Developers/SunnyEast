namespace Domain.Entities;

public class PhoneOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PhoneE164 { get; set; } = default!;
    public string Purpose { get; set; } = default!;
    public string CodeHash { get; set; } = default!;
    public string Salt { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? BlockedUntil { get; set; }
    public int Attempts { get; set; } = 0;
    public DateTimeOffset? LastSentAt { get; set; }
}