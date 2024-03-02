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
        var old = await context.ProductCategories.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);
        if (old == null) throw new NotFoundException($"ProductCategory with id {request.Slug} is not found");

        request.Name = request.Name.Trim();

        if (await context.ProductCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower(),
                cancellationToken) != null)
            throw new ExistException($"ProductCategory with name {request.Name} is already exists");

        old.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);
        return old.Slug;
    }
}