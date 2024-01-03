namespace Application.Contract.Level.Responses;

public record LevelResponse
{
    public required string Slug { get; set; }
    public string Name { get; set; } = "";
}