namespace Application.Contract.Shops.Commands;

public class DeleteShopCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}