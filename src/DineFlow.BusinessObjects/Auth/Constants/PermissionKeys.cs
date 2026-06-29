namespace DineFlow.BusinessObjects.Auth.Constants;

public static class PermissionKeys
{
    // User & Role Management
    public const string ManageUsers = "ManageUsers";

    // Table & QR & Session Management
    public const string ViewTables = "ViewTables";
    public const string ManageTables = "ManageTables";
    public const string ManageQr = "ManageQr";
    public const string ViewTableSessions = "ViewTableSessions";

    // Menu & Category Management
    public const string ManageCategories = "ManageCategories";
    public const string ManageMenuItems = "ManageMenuItems";
    public const string UpdateStock = "UpdateStock";
    public const string ToggleItemAvailability = "ToggleItemAvailability";

    // Order Management
    public const string ViewOrders = "ViewOrders";
    public const string PrintOrders = "PrintOrders";
    public const string CancelOrders = "CancelOrders";

    // Service & Payment Requests
    public const string HandleServiceRequests = "HandleServiceRequests";

    // Bill & Payment
    public const string ViewBills = "ViewBills";
    public const string SplitBills = "SplitBills";
    public const string ConfirmPayment = "ConfirmPayment";
    public const string UpdatePaymentMethod = "UpdatePaymentMethod";

    // Dashboard
    public const string ViewDashboard = "ViewDashboard";
}
