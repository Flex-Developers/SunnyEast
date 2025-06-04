using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItems.Commands;

public class UpdateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderCommand, string>
{
    public async Task<string> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CartItems.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order with id {request.Slug} is not found");

        order.Gr100SelectedCount = request.Gr100SelectedCount;
        order.Gr300SelectedCount = request.Gr300SelectedCount;
        order.Gr500SelectedCount = request.Gr500SelectedCount;
        order.Gr1000SelectedCount = request.Gr1000SelectedCount;
        order.Gr2000SelectedCount = request.Gr2000SelectedCount;
        order.Gr3000SelectedCount = request.Gr3000SelectedCount;
        order.Gr5000SelectedCount = request.Gr5000SelectedCount;
        order.Ml100SelectedCount = request.Ml100SelectedCount;
        order.Ml300SelectedCount = request.Ml300SelectedCount;
        order.Ml500SelectedCount = request.Ml500SelectedCount;
        order.Ml1000SelectedCount = request.Ml1000SelectedCount;
        order.Ml2000SelectedCount = request.Ml2000SelectedCount;
        order.Ml3000SelectedCount = request.Ml3000SelectedCount;
        order.Ml5000SelectedCount = request.Ml5000SelectedCount;
        order.OneSelectedCount = request.OneSelectedCount;
        order.TwoSelectedCount = request.TwoSelectedCount;
        order.ThreeSelectedCount = request.ThreeSelectedCount;
        order.FiveSelectedCount = request.FiveSelectedCount;

        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}