using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Customer.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class DeleteCustomerCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteLevelCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLevelCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(s => s.Slug == request.Slug,
            cancellationToken);

        if (customer != null)
        {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new NotFoundException($"customer with slug {request.Slug} not found");
    }
}