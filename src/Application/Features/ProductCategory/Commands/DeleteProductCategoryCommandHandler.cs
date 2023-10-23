using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category =
            await _context.ProductCategories.FirstOrDefaultAsync(s => s.Id == request.Id,
                cancellationToken);
        if (category == null) throw new NotFoundException("Product Category not found");

        _context.ProductCategories.Remove(category);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}