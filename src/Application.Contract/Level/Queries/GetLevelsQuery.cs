using Application.Contract.Level.Responses;

namespace Application.Contract.Level.Queries;

public class GetLevelsQuery : IRequest<List<LevelResponse>>
{
    public string? Slug { get; set; }
    public string? Name { get; set; }
}