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
            var s = volume.Trim().ToLowerInvariant();
            
            if (s.Contains("шт")) 
                return VolumeGroup.Piece;
            if (s.Contains(" кг") || s.EndsWith("кг") || s.Contains(" г") || s.EndsWith("г")) 
                return VolumeGroup.Mass;
            if (s.Contains(" л") || s.EndsWith("л") || s.Contains(" мл") || s.EndsWith("мл")) 
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