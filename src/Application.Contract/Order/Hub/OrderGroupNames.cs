namespace Application.Contract.Order.Hub;

/// <summary>
/// Единое место, где формируются имена групп для OrderHub.
/// Расположено в Application.Contract, поэтому допускается
/// зависимость как из Application, так и из Infrastructure.
/// </summary>
public static class OrderGroupNames
{
    public const string SuperAdminsGroup = "superadmins";
    public static string Shop(Guid shopId) => $"shop:{shopId}";
    public static string Customer(Guid customerId) => $"customer:{customerId}";
}