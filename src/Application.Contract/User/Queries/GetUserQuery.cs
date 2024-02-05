using Application.Contract.User.Responses;

namespace Application.Contract.User.Queries;

public class GetUserQuery : IRequest<List<CustomerResponse>>
{
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
}