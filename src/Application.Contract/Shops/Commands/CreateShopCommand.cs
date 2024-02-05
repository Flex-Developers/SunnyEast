namespace Application.Contract.Shops.Commands;

public class CreateShopCommand : IRequest<string>
{
    public required string Slug { get; set; }
    public required string Address { get; set; }
    public string? AddressGoogle { get; set; }
    public string? AddressYandex { get; set; }
}