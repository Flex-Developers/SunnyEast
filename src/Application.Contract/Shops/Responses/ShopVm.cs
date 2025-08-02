namespace Application.Contract.Shops.Responses;

public sealed class ShopVm
{
    public required string Slug { get; set; }
    public required string Address { get; set; }
    public string[]? Images { get; set; }
}