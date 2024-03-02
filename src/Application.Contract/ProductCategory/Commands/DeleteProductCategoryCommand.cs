namespace Application.Contract.ProductCategory.Commands;

public class DeleteProductCategoryCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}