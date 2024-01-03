namespace Application.Contract.Customer.Commands;

public class DeleteLevelCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}