namespace HelperLibrary; 

public static class NumberHelpers {
    
    public static int GetRandomNumberInRangeInclusive(int min, int max) => new Random().Next(min, max + 1);
    
    public static bool IsNumber(string any) => int.TryParse(any, out _);
}