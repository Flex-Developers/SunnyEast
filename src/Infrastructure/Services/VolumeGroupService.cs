using Application.Common.Interfaces.Services;

namespace Infrastructure.Services;

public class VolumeGroupService : IVolumeGroupService
{
    public VolumeGroup Detect(string volume)
    {
        var lowerInvariant = (volume ?? string.Empty).Trim().ToLowerInvariant();

        if (lowerInvariant.Contains("шт"))
            return VolumeGroup.Piece;

        if (lowerInvariant.Contains(" кг") || lowerInvariant.EndsWith("кг") || lowerInvariant.Contains(" г") || lowerInvariant.EndsWith("г"))
            return VolumeGroup.Mass;

        if (lowerInvariant.Contains(" л") || lowerInvariant.EndsWith("л") || lowerInvariant.Contains(" мл") || lowerInvariant.EndsWith("мл"))
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