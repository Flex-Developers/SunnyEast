namespace Client.Infrastructure.Services.Category;

public interface ICategoryVolumesValidationService
{
    bool VolumesAreFromOneGroup(IEnumerable<string> volumes, out string? error);
}