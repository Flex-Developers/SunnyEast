using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.User.Commands;

public class RegisterUserCommand : IRequest<string>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string Patronymic { get; set; } = string.Empty;
    [Phone]
    public string? Phone { get; set; }
    public required string UserName { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    public required string Password { get; set; }
}