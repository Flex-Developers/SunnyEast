using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class UpdateProductCategoryCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    IMapper mapper)
    : IRequestHandler<UpdateProductCategoryCommand, string>
{
    public async Task<string> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var old = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (old is null)
            throw new NotFoundException($"Категория не найдена {request.Slug}");

        old.Name = request.Name.Trim();
        old.Slug = slugService.GenerateSlug(request.Name);

        mapper.Map(request, old);

        await context.SaveChangesAsync(cancellationToken);
        return old.Slug;
    }
}