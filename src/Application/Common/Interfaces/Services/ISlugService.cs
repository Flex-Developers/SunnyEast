namespace Application.Common.Interfaces.Services;

public interface ISlugService
{
    public string GenerateSlug(string raw);
}