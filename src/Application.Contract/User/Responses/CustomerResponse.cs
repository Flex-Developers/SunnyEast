namespace Application.Contract.User.Responses;

public record CustomerResponse
{
    public required string UserName { get; set; }
    public required string LevelSlug { get; set; }
    public string Name { get; set; } = "";
    public string Surname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}