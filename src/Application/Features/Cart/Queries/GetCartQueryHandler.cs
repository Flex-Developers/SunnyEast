using Application.Common.Interfaces.Contexts;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart.Queries;

public class GetCartQueryHandler(IApplicationDbContext context, IMapper mapper)
: IRequestHandler<GetCartQuery,CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);
        var response = mapper.Map<CartResponse>(cart);

        return response;
    }
}