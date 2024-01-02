namespace Application.Contract.Customer.Responses;

public record CustomerResponse
{
    public required string Slug { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}