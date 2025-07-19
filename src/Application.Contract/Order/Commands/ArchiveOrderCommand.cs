using Application.Contract.Enums;

namespace Application.Contract.Order.Commands;

public class ArchiveOrderCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public required OrderStatus CurrentStatus { get; set; }
}