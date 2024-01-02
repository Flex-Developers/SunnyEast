using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductCategory.Commands;

public class CreateProductCategoryCommandHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<CreateProductCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var productCategory = mapper.Map<CreateProductCategoryCommand, Domain.Entities.ProductCategory>(request);

        productCategory.Name = productCategory.Name.Trim();
        await context.ProductCategories.AddAsync(productCategory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return productCategory.Id;
    }
}