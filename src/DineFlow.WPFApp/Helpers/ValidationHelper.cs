namespace DineFlow.WPFApp.Helpers;

public static class ValidationHelper
{
    public static bool IsPositiveInt(string value)
    {
        return int.TryParse(value, out var number) && number > 0;
    }
}
