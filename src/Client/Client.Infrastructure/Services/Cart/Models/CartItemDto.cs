namespace Client.Infrastructure.Services.Cart.Models;

public class CartItemDto
{
    public required string ProductSlug { get; set; }
    public string? ProductName { get; set; }
    public string? SelectedVolume { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
}