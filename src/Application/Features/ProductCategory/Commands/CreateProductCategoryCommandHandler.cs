using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductCategory.Commands;

public class CreateProductCategoryCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ISlugService slugService)
    : IRequestHandler<CreateProductCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var productCategory = mapper.Map<CreateProductCategoryCommand, Domain.Entities.ProductCategory>(request);
        productCategory.Slug = slugService.GenerateSlug(request.Name);
        if (context.ProductCategories.Any(s => s.Slug == productCategory.Slug))
            throw new ExistException($"the product with slug {productCategory.Slug} already exist");
        productCategory.Name = productCategory.Name.Trim();
        await context.ProductCategories.AddAsync(productCategory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return productCategory.Id;
    }
}