namespace Application.Common.Interfaces.Services;

public interface IVolumeGroupService
{
    VolumeGroup Detect(string volume);
    bool AreFromSameGroup(IEnumerable<string>? volumes, out VolumeGroup? group, out string? error);
}

public enum VolumeGroup
{
    Piece,   // шт
    Mass,    // г, кг
    Liquid   // мл, л
}