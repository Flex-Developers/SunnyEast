using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Customer.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class UpdateCustomerCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCustomerCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);
        if (customer == null) throw new NotFoundException($"Customer with slug {request.Slug}");
        customer.Name = request.Name ?? customer.Name;
        customer.Phone = request.Phone ?? customer.Phone;
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}