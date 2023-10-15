namespace Application.Contract.ProductCategory.Commands;

#nullable disable
public class CreateProductCategoryCommand : IRequest<Guid>
{
    public string Name { get; set; }
}