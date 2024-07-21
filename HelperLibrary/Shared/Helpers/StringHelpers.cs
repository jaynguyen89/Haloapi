using System.Text.RegularExpressions;

namespace HelperLibrary.Shared.Helpers;
    
public static class StringHelpers {

    public static string NewGuid(bool longerId = false) => longerId ? $"{NewGuid()}{NewGuid()}" : Guid.NewGuid().ToString("N");

    public static bool IsString(this string? any) => !string.IsNullOrEmpty(any) && !string.IsNullOrWhiteSpace(any);
        
    public static string RemoveAllSpaces(this string any) => Regex.Replace(any, Constants.MultiSpace, string.Empty);
    
    /*
     * Splits a Camel-Case string at capital letters and returns spaced string.
     * Eg. ThisIsAnExample -> This Is An Example
     */
    public static string Lucidify(this string any) => string.Join(Constants.MonoSpace, Regex.Split(any, @"(?<!^)(?=[A-Z])"));

    public static string CapitalizeFirstLetterOfEachWord(this string sentence) => Regex.Replace(sentence, @"(^\w)|(\s\w)", m => m.Value.ToUpper());

    public static string LowerCaseFirstChar(this string any) => char.ToLower(any[0]) + any[1..];

    public static string UpperCaseFirstChar(this string any, bool restToLower = false) => char.ToUpper(any[0]) + (restToLower ? any[1..].ToLower() : any[1..]);
    
    public static string RemoveAllSpecialChars(this string any) => Regex.Replace(any, "[^a-zA-Z0-9 ]+", string.Empty, RegexOptions.Compiled);
    
    public static string GenerateRandomString(int length = Constants.RandomStringDefaultLength, bool includeSpecialChars = false, bool caseSensitive = true) {
        const string sChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890!@$%-_+.*|:<>";
        const string nChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
        const string scChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890!@$%-_+.*|:<>";
        const string ncChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890";
        
        var charSetToUse = caseSensitive
            ? (includeSpecialChars ? sChars : nChars)
            : (includeSpecialChars ? scChars : ncChars);
        
        var randomString = new string(
            Enumerable.Repeat(charSetToUse, length)
                      .Select(p => p[new Random().Next(p.Length)])
                      .ToArray()
        );

        return randomString;
    }

    public static string SplitToGroups(this string any, int numberOfGroups, string separator = Constants.Hyphen) {
        if (numberOfGroups < 2) return any;
        
        var groupLength = any.Length / numberOfGroups;
        return Enumerable.Range(0, numberOfGroups)
            .Select(x => {
                var start = x * groupLength;
                var length = x != numberOfGroups - 1 ? groupLength : (
                    (x + 1) * groupLength == any.Length ? groupLength : any.Length - x * groupLength
                );
                return any.Substring(start, length);
            })
            .Aggregate((x, y) => $"{x}{separator}{y}");
    }

    public static string ShortOrLong(this string any, params int[] bounds) => any.Length < bounds[0] ? "short" : (any.Length > bounds[1] ? "long" : string.Empty);
    
    public static bool IsValidUrl(this string url) => Uri.TryCreate(url, UriKind.Absolute, out _);

    public static string SetDefaultEmailBodyValues(this string body, Tuple<string, string, string> defaultBodyValues) {
        var (halogenLogoUrl, clientBaseUri, clientApplicationName) = defaultBodyValues;
        body = Regex.Replace(body, @"\bCLIENT_LOGO_URL\b", halogenLogoUrl);
        body = Regex.Replace(body, @"\bCLIENT_BASE_URI\b", clientBaseUri);
        body = Regex.Replace(body, @"\bCLIENT_APPLICATION_NAME\b", clientApplicationName);
        body = Regex.Replace(body, @"\bCURRENT_YEAR\b", DateTime.UtcNow.Year.ToString());
        return body;
    }
}
