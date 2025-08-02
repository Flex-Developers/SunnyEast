namespace Application.Contract.Identity;

public static class ApplicationRoles
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string Administrator = nameof(Administrator);
    public const string Salesman = nameof(Salesman);
    public const string Customer = nameof(Customer);
    
    public const string AllStaffCsv = $"{SuperAdmin}, {Administrator}, {Salesman}";
    public const string AllAdmins = $"{SuperAdmin}, {Administrator}";
}