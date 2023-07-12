namespace HelperLibrary; 

public static class ListHelper {
    public static TV? GetDictionaryValue<TK, TV>(this Dictionary<TK, TV?> dictionary, TK key) where TK : notnull => !dictionary.TryGetValue(key, out var value) ? default : value;
}