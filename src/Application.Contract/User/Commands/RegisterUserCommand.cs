
namespace Application.Contract.User.Commands;

public class RegisterUserCommand : IRequest<string>
{
    public required string Name { get; set; }
    public string Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}