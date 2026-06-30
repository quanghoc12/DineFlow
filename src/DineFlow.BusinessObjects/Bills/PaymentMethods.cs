namespace DineFlow.BusinessObjects.Bills;

public static class PaymentMethods
{
    public const string Cash = "Cash";
    public const string BankTransfer = "BankTransfer";
    public const string Card = "Card";
    public const string Combined = "Combined";

    public static readonly string[] StoredValues = [Cash, BankTransfer, Card];
    public static readonly string[] RequestValues = [Cash, BankTransfer, Card, Combined];

    public static bool IsStoredValue(string? paymentMethod) =>
        StoredValues.Contains(paymentMethod, StringComparer.Ordinal);

    public static bool IsRequestValue(string? paymentMethod) =>
        RequestValues.Contains(paymentMethod, StringComparer.Ordinal);
}
