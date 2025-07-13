namespace Application.Contract.Order.Commands;

public class CreateOrderItem
{
    public required string ProductSlug { get; set; }
    public int Quantity { get; set; }
    public string? SelectedVolume { get; set; }
}
