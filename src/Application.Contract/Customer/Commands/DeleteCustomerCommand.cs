namespace Application.Contract.Customer.Commands;

public class DeleteCustomerCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}