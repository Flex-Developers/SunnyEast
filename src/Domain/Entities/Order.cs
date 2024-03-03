using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// one order on card
/// </summary>
public sealed class Order : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public required string CartSlug { get; set; }   

    //какой продукт покупаем
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal SummaryPrice { get; set; } //quantity * product.Price

    public OrderStatus Status { get; set; }
    public required string Slug { get; set; }
}