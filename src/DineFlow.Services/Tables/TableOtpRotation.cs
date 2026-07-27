using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables;

public static class TableOtpRotation
{
    public static void Rotate(DiningTable table, DateTime? rotatedAt = null)
    {
        DateTime now = rotatedAt ?? DateTime.UtcNow;
        table.CurrentOtp = TableOtpGenerator.Generate();
        table.OtpUpdatedAt = now;
        table.UpdatedAt = now;
    }
}
