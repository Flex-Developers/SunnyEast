using Application.Contract.Customer.Responses;

namespace Application.Contract.Customer.Queries;

public class GetCustomersQuery : IRequest<List<CustomerResponse>>
{
    public string? Slug { get; set; }
    public string? LevelSlug { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
}