using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class DeleteProductCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProductCategoryCommand>
{
    public async Task<Unit> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);
        
        if (category is null)
            throw new NotFoundException($"Категория не найдена {request.Slug}");

        context.ProductCategories.Remove(category);

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}