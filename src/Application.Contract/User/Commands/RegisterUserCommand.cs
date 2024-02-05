namespace Application.Contract.User.Commands;

public class RegisterUserCommand : IRequest<string>
{
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}