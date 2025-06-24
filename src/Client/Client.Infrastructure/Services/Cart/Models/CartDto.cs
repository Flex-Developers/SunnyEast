namespace Client.Infrastructure.Services.Cart.Models;

public class CartDto
{
    public string Slug { get; set; } = Guid.NewGuid().ToString();
    public List<CartItemDto> Orders { get; set; } = new();
}