namespace HelperLibrary.Shared.Helpers; 

public static class NumberHelpers {

    public static int GetRandomNumberInRangeInclusive(int min, int max) {
        if (min >= max) throw new ArgumentException("Invalid range: min should be less than max.");
        return new Random().Next(min, max + 1);
    }

    public static bool IsNumber(this string any) => any.IsString() && (int.TryParse(any, out _) || double.TryParse(any, out _));

    public static int? ToInt(this string? any) => !any!.IsNumber() ? null : int.Parse(any!);

    public static double? ToDouble(this string? any) => !any!.IsNumber() ? null : double.Parse(any!);
}