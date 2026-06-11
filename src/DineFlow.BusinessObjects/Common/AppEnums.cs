namespace DineFlow.BusinessObjects.Common;

public enum UserRole
{
    Admin = 1,
    Staff = 2
}

public enum DiningTableStatus
{
    Available = 1,
    Occupied = 2,
    WaitingPayment = 3
}

public enum TableSessionStatus
{
    Open = 1,
    WaitingPayment = 2,
    Closed = 3,
    Cancelled = 4
}

public enum OrderSource
{
    CustomerWeb = 1,
    StaffApp = 2
}

public enum OrderStatus
{
    Accepted = 1,
    Cancelled = 2
}

public enum PrintStatus
{
    PendingPrint = 1,
    Printed = 2,
    PrintFailed = 3
}

public enum ServiceRequestType
{
    CallStaff = 1,
    PaymentRequest = 2
}

public enum ServiceRequestStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3
}

public enum BillStatus
{
    Unpaid = 1,
    Paid = 2,
    Cancelled = 3
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Card = 3,
    EWallet = 4
}
