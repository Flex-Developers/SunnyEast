namespace Application.Contract.Shops.Responses;

public class ShopResponse
{
    public required string Slug { get; set; }
    public required string Address { get; set; }
    public string[]? Images { get; set; }
    public List<ShopStaffBriefResponse>? Staff { get; set; }
}