namespace Domain.Entities;

public sealed class Shop : BaseEntity
{
    public string? Address { get; set; }
    public string? AddressGoogle { get; set; }
    public string? AddressYandex { get; set; }

    public List<ShopOrder>? ShopOrders { get; set; }
}