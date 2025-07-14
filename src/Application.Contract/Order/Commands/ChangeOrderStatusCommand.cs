using Application.Contract.Enums;

namespace Application.Contract.Order.Commands;

public class ChangeOrderStatusCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public required OrderStatus Status { get; set; }
    public string? Comment { get; set; } 
}