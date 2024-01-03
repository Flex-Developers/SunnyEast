namespace Application.Contract.Level.Commands;

public class UpdateLevelCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
}