namespace Application.Contract.User.Responses;

public record CustomerResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; } = "";
    public string Surname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public bool IsStaff { get; set; }
}