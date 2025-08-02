namespace Application.Contract.Account.Responses;

public sealed class MyAccountResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; } // Заглушка на будущее
}