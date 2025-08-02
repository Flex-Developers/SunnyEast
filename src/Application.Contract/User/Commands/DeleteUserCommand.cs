namespace Application.Contract.User.Commands;

public sealed class DeleteUserCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}