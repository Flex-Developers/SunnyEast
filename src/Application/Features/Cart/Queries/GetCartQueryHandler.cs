using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart.Queries;

public class GetCartQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetCartQuery, CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);

        if (cart == null)
            throw new NotFoundException($"The cart with slug: {request.Slug} not found.");
        
        var response = mapper.Map<CartResponse>(cart);
        response.Sum = cart.Orders!.Select(p => p.Product!.Price).Sum(); // Maybe will change to Order.SummaryPrice in future

        return response;
    }
}