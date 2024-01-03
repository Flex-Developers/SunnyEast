namespace Application.Contract.Level.Commands;

public class DeleteLevelCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}