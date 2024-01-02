using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Customer.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class CreateCustomerCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService)
    : IRequestHandler<CreateCustomerCommand, string>
{
    public async Task<string> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = mapper.Map<Customer>(request);
        customer.Slug = slugService.GenerateSlug(customer.Name);
        if (await context.Customers.AnyAsync(s => s.Slug == customer.Slug, cancellationToken))
            throw new ExistException($"Customer with name {customer.Name} and slug {customer.Slug} already exist");

        await context.Customers.AddAsync(customer, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return customer.Slug;
    }
}