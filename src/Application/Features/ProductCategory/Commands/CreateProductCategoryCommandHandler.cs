using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductCategory.Commands;

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateProductCategoryCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var productCategory = _mapper.Map<CreateProductCategoryCommand, Domain.Entities.ProductCategory>(request);

        productCategory.Name = productCategory.Name.Trim();

        await _context.ProductCategories.AddAsync(productCategory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return productCategory.Id;
    }
}