namespace DineFlow.BusinessObjects.Auth;

public static class AuthRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Staff = "Staff";

    public static bool IsOwner(string? role) =>
        string.Equals(role, Owner, StringComparison.OrdinalIgnoreCase);

    public static bool IsAdmin(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);

    public static bool IsStaff(string? role) =>
        string.Equals(role, Staff, StringComparison.OrdinalIgnoreCase);

    public static bool CanManage(string? role) => IsOwner(role) || IsAdmin(role);

    public static string Normalize(string role) =>
        IsOwner(role) ? Owner : IsAdmin(role) ? Admin : Staff;

    public static string GetLabel(string? role) =>
        IsOwner(role) ? "Chủ nhà hàng" : IsAdmin(role) ? "Admin" : "Staff";
}
