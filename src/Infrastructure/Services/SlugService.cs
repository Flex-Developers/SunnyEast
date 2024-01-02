using Application.Common.Interfaces.Services;
using Slugify;

namespace Infrastructure.Services;

public class SlugService : ISlugService
{
    private static readonly SlugHelperConfiguration Configuration = new()
    {
        StringReplacements = new Dictionary<string, string>
        {
            { ".", "_" }
        },
        ForceLowerCase = false,
        CollapseWhiteSpace = true,
        DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]",
        CollapseDashes = true,
        TrimWhitespace = true
    };

    private readonly SlugHelper _slugHelper = new(Configuration);

    public string GenerateSlug(string raw)
    {
        return _slugHelper.GenerateSlug(raw);
    }
}