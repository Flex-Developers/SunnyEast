namespace Domain.Entities;

public class Customer : BaseEntity
{
    public required Guid LevelId { get; set; }
    public required string LevelSlug { get; set; }
    public Level? Level { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }

    // public string LName { get; set; }
    // public string Patronymic { get; set; }
    public string? Phone { get; set; }
}