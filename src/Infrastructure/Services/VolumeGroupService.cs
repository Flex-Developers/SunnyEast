using Application.Common.Interfaces.Services;

namespace Infrastructure.Services;

public class VolumeGroupService : IVolumeGroupService
{
    public VolumeGroup Detect(string volume)
    {
        var s = (volume ?? string.Empty).Trim().ToLowerInvariant();

        if (s.Contains("шт"))
            return VolumeGroup.Piece;

        if (s.Contains(" кг") || s.EndsWith("кг") || s.Contains(" г") || s.EndsWith("г"))
            return VolumeGroup.Mass;

        if (s.Contains(" л") || s.EndsWith("л") || s.Contains(" мл") || s.EndsWith("мл"))
            return VolumeGroup.Liquid;

        // по умолчанию считаем штучным
        return VolumeGroup.Piece;
    }

    public bool AreFromSameGroup(IEnumerable<string>? volumes, out VolumeGroup? group, out string? error)
    {
        error = null;
        group = null;

        if (volumes is null)
            return true;

        var distinct = volumes
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(Detect)
            .Distinct()
            .ToList();

        if (distinct.Count <= 1)
        {
            group = distinct.FirstOrDefault();
            return true;
        }

        error = "Все выбранные объёмы должны быть из одной группы: шт / г‑кг / мл‑л.";
        return false;
    }
}