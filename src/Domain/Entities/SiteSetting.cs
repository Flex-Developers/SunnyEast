namespace Domain.Entities;

public sealed class SiteSetting : BaseEntity
{
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public string? TelegramLink { get; set; }
    public string? WhatsAppLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? FacebookLink { get; set; }
    public string? Copyright { get; set; }
}
