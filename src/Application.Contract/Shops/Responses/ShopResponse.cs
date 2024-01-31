namespace Application.Contract.Shops.Responses;

public class ShopResponse
{
    public required string Slug { get; set; }
    public required string Address { get; set; }
    public string? AddressGoogle { get; set; }
    public string? AddressYandex { get; set; }
}