namespace Application.Contract.Customer.Commands;

public class UpdateCustomerCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? LevelSlug { get; set; }
}