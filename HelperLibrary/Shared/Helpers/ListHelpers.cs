namespace HelperLibrary.Shared.Helpers; 

public static class ListHelpers {
    public static TV? GetDictionaryValue<TK, TV>(this Dictionary<TK, TV?> dictionary, TK key) where TK : notnull {
        if (!dictionary.ContainsKey(key)) throw new KeyNotFoundException($"Key \"{key}\" does not exist in dictionary.");
        return dictionary.GetValueOrDefault(key);
    }

    public static Dictionary<string, List<string>> MergeDataValidationErrors(params KeyValuePair<string, List<string>>[] fieldErrorsPairs) {
        var allFieldErrors = new Dictionary<string, List<string>>();
        
        foreach (var (field, errors) in fieldErrorsPairs)
            if (errors.Count > 0) allFieldErrors.Add(field, errors);

        return allFieldErrors;
    }

    public static void MergeDataValidationErrors(this Dictionary<string, List<string>> allErrors, params KeyValuePair<string, List<string>>[] fieldErrorsPairs) {
        foreach (var (field, errors) in fieldErrorsPairs)
            if (errors.Count > 0) allErrors.Add(field, errors);
    }
}