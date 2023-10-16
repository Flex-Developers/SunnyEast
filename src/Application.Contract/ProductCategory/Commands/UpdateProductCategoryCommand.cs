namespace Application.Contract.ProductCategory.Commands;

#nullable disable

public class UpdateProductCategoryCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}