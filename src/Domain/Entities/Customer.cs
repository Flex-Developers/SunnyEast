namespace Domain.Entities;

public class Customer : BaseEntity
{
    public required Guid LevelId { get; set; }
    public required string LevelSlug { get; set; }
    public Level? Level { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }

    // public string LName { get; set; }
    // public string Patronymic { get; set; }
    public string? Phone { get; set; }

    public int ProductsPurchasedCount { get; set; }
    public decimal ProductsPurchasedSum { get; set; }

    public int SellProductsPurchasedCount { get; set; }
    public decimal SellProductsPurchasedSum { get; set; }

    public int DiscountProductsPurchasedCount { get; set; }
    public decimal DiscountProductsPurchasedSum { get; set; }

    public int PurchasedTotalCount { get; set; }
    public decimal PurchasedTotalSum { get; set; }

    public decimal TotalIncome { get; set; }
}