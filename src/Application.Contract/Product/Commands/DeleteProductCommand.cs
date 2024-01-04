namespace Application.Contract.Product.Commands;

public class DeleteProductCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}