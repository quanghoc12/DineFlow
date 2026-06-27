using DineFlow.BusinessObjects.Auth.Constants;
using DineFlow.Services.Interfaces;

namespace DineFlow.Services.Impl;

public class PermissionService : IPermissionService
{
    private readonly ICurrentUserService _currentUserService;

    // Define permissions per role in memory
    private readonly Dictionary<string, HashSet<string>> _rolePermissions;

    public PermissionService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;

        _rolePermissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                Roles.Admin, new HashSet<string>
                {
                    PermissionKeys.ManageUsers,
                    PermissionKeys.ViewTables,
                    PermissionKeys.ManageTables,
                    PermissionKeys.ManageQr,
                    PermissionKeys.ViewTableSessions,
                    PermissionKeys.ManageCategories,
                    PermissionKeys.ManageMenuItems,
                    PermissionKeys.UpdateStock,
                    PermissionKeys.ToggleItemAvailability,
                    PermissionKeys.ViewOrders,
                    PermissionKeys.PrintOrders,
                    PermissionKeys.CancelOrders,
                    PermissionKeys.HandleServiceRequests,
                    PermissionKeys.ViewBills,
                    PermissionKeys.SplitBills,
                    PermissionKeys.ConfirmPayment,
                    PermissionKeys.UpdatePaymentMethod,
                    PermissionKeys.ViewDashboard
                }
            },
            {
                Roles.Staff, new HashSet<string>
                {
                    PermissionKeys.ViewTables,
                    PermissionKeys.ViewTableSessions,
                    PermissionKeys.ViewOrders,
                    PermissionKeys.PrintOrders,
                    PermissionKeys.CancelOrders,
                    PermissionKeys.HandleServiceRequests,
                    PermissionKeys.UpdateStock,
                    PermissionKeys.ToggleItemAvailability,
                    PermissionKeys.ViewBills,
                    PermissionKeys.SplitBills,
                    PermissionKeys.ConfirmPayment,
                    PermissionKeys.ViewDashboard
                }
            }
        };
    }

    public bool HasPermission(string permissionKey)
    {
        if (!_currentUserService.IsAuthenticated()) return false;

        var role = _currentUserService.GetRole();
        if (string.IsNullOrEmpty(role)) return false;

        if (_rolePermissions.TryGetValue(role, out var permissions))
        {
            return permissions.Contains(permissionKey);
        }

        return false;
    }

    public void RequirePermission(string permissionKey)
    {
        if (!HasPermission(permissionKey))
        {
            throw new UnauthorizedAccessException($"You do not have the required permission: {permissionKey}");
        }
    }

    public IEnumerable<string> GetCurrentPermissions()
    {
        if (!_currentUserService.IsAuthenticated()) return Enumerable.Empty<string>();

        var role = _currentUserService.GetRole();
        if (string.IsNullOrEmpty(role)) return Enumerable.Empty<string>();

        if (_rolePermissions.TryGetValue(role, out var permissions))
        {
            return permissions;
        }

        return Enumerable.Empty<string>();
    }
}
