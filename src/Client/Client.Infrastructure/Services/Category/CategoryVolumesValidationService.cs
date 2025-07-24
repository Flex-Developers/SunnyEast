namespace Client.Infrastructure.Services.Category;

public class CategoryVolumesValidationService : ICategoryVolumesValidationService
{
    public bool VolumesAreFromOneGroup(IEnumerable<string> volumes, out string? error)
    {
        error = null;
        if (volumes is null) 
            return true;

        VolumeGroup? volumeGroup = null;

        foreach (var v in volumes.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var current = Detect(v);
            if (volumeGroup == null) 
                volumeGroup = current;
            else if (volumeGroup != current)
            {
                error = "Все выбранные объёмы должны быть из одной группы: шт / г‑кг / мл‑л.";
                return false;
            }
        }

        return true;

        VolumeGroup Detect(string volume)
        {
            var lowerInvariant = volume.Trim().ToLowerInvariant();
            
            if (lowerInvariant.Contains("шт")) 
                return VolumeGroup.Piece;
            if (lowerInvariant.Contains(" кг") || lowerInvariant.EndsWith("кг") || lowerInvariant.Contains(" г") || lowerInvariant.EndsWith("г")) 
                return VolumeGroup.Mass;
            if (lowerInvariant.Contains(" л") || lowerInvariant.EndsWith("л") || lowerInvariant.Contains(" мл") || lowerInvariant.EndsWith("мл")) 
                return VolumeGroup.Liquid;
            
            return VolumeGroup.Piece;
        }
    }

    private enum VolumeGroup
    {
        Piece, 
        Mass,
        Liquid
    }
}