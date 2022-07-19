namespace HelperLibrary; 

public static class NumberHelpers {
    
    public static int GetRandomNumberInRangeInclusive(int min, int max) => new Random().Next(min, max + 1);
    
    public static bool IsNumber(string any) => int.TryParse(any, out _);

    public static int? ToInt(this string? any) => !any.IsString() ? default : int.TryParse(any, out var result) ? result : default;

    public static double? ToDouble(this string? any) => !any.IsString() ? default : double.TryParse(any, out var result) ? result : default;
}