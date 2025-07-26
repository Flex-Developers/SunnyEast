namespace Domain.Entities;

public sealed class Staff : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public Guid? ShopId { get; set; }
    public Shop? Shop { get; set; }

    public bool IsActive { get; set; } = true;
    public string? StaffRole { get; set; } // Например, "Администратор", "Менеджер", "Кассир" и т.д.
}