namespace Application.Contract.Level.Commands;

public class CreateLevelCommand : IRequest<string>
{
    public required string Name { get; set; }
}