using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        if (await _context.ProductCategories.FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower(),
                cancellationToken) != null)
            throw new ExistException();

        var productCategory = _mapper.Map<CreateProductCategoryCommand, Domain.Entities.ProductCategory>(request);

        await _context.ProductCategories.AddAsync(productCategory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return productCategory.Id;
    }
}