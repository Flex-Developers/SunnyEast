using Application.Common.Interfaces.Contexts;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart.Queries;

public class GetCartsQueryHandler(IApplicationDbContext context, IMapper mapper)
: IRequestHandler<GetCartsQuery, List<CartResponse>>
{
    public async Task<List<CartResponse>> Handle(GetCartsQuery request, CancellationToken cancellationToken)
    {
        var carts = await context.Carts.Where(c => c.ShopSlug == request.ShopSlug)
            .Select(c => mapper.Map<CartResponse>(c)).ToListAsync(cancellationToken);

        return carts;
    }
}