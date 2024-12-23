namespace Domain.Entities;

public sealed class Shop : BaseEntity
{
    public required string Slug { get; set; }
    public required string Address { get; set; }
    public string? AddressGoogle { get; set; }
    public string? AddressYandex { get; set; }
    
    public string[]? Images { get; set; }

    public List<ShopOrder>? ShopOrders { get; set; }
}