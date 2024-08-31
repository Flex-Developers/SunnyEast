using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class UpdateProductCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProductCategoryCommand, string>
{
    public async Task<string> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var old = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);
        
        if (old is null) 
            throw new NotFoundException($"Категория не найдена {request.Slug}");

        request.Name = request.Name.Trim();

        if (await context.ProductCategories.FirstOrDefaultAsync(c => c.Name.Equals(request.Name, StringComparison.CurrentCultureIgnoreCase),
                cancellationToken) != null)
            throw new ExistException($"Категория с названием {request.Name} уже существует!");

        old.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);
        return old.Slug;
    }
}