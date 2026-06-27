namespace DineFlow.Services.Interfaces;

public interface IPermissionService
{
    bool HasPermission(string permissionKey);
    void RequirePermission(string permissionKey);
    IEnumerable<string> GetCurrentPermissions();
}
