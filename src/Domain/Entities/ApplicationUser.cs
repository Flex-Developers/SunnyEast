using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string Name { get; set; }
    public string Surname { get; set; } = string.Empty;
}