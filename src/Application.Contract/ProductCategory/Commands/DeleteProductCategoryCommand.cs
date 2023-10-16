namespace Application.Contract.ProductCategory.Commands;

public class DeleteProductCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}