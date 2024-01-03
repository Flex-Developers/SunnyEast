namespace Application.Contract.Customer.Commands;

public class CreateCustomerCommand : IRequest<string>
{
    public required string Name { get; set; }
    public required string LevelSlug { get; set; }
    public string? Phone { get; set; }
}