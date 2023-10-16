using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var old = await _context.ProductCategories.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (old == null) throw new NotFoundException($"ProductCategory with id {request.Id} is not found");

        if (await _context.ProductCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower(),
                cancellationToken) != null)
            throw new ExistException($"ProductCategory with name {request.Name} is already exists");

        old.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return old.Id;
    }
}