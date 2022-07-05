using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace HelperLibrary;
    
public static class Helpers {
    
    public static byte[] EncodeDataAscii(this object data) => Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
    
    public static byte[] EncodeDataUtf8(this object data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

    public static T? DecodeUtf8<T>(this byte[] data) {
        var decodedData = Encoding.UTF8.GetString(data);
        return typeof(T).IsPrimitive
            ? (T) Convert.ChangeType(decodedData, typeof(T))
            : JsonConvert.DeserializeObject<T>(decodedData);
    }
    
    /// <summary>
    /// To get StringValue and ByteValue of enums having EnumValue attribute.
    /// </summary>
    public static T? GetEnumValue<T>(this Enum any) {
        var enumType = any.GetType();
        var enumFields = enumType.GetField(any.ToString());

        if (enumFields is null) return default;

        if (typeof(T).Name.ToLower().Equals(nameof(String).ToLower())) {
            var stringValue = enumFields.GetCustomAttributes(typeof(EnumValueAttribute), false)
                is EnumValueAttribute[] { Length: > 0 } attributesForStringValue
                ? attributesForStringValue[0].StringValue
                : string.Empty;
                
            return string.IsNullOrEmpty(stringValue)
                ? default
                : (T) Convert.ChangeType(stringValue, TypeCode.String);
        }

        var byteValue = enumFields.GetCustomAttributes(typeof(EnumValueAttribute), false)
            is EnumValueAttribute[] { Length: > 0 } attributesForByteValue
            ? attributesForByteValue[0].ByteValue
            : byte.MinValue;
            
        return (T) Convert.ChangeType(byteValue, TypeCode.Byte);
    }
    
    public static bool IsString(string? any) => !string.IsNullOrEmpty(any) && !string.IsNullOrWhiteSpace(any);
        
    public static string RemoveAllSpaces(this string any) => Regex.Replace(any, Constants.MultiSpace, string.Empty);
    
    /*
     * Splits a Camel-Case string at capital letters and returns spaced string.
     * Eg. ThisIsAnExample -> This Is An Example
     */
    public static string ToHumanStyled(this string any) => string.Join(Constants.MonoSpace, Regex.Split(any, @"(?<!^)(?=[A-Z])"));
    
    public static int GetRandomNumberInRangeInclusive(int min, int max) => new Random().Next(min, max + 1);
    
    public static string CapitalizeFirstLetterOfEachWord(this string sentence) => Regex.Replace(sentence, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
    
    public static bool IsNumber(string any) => int.TryParse(any, out _);
    
    public static string LowerCaseFirstChar(this string any) => char.ToLower(any[0]) + any[1..];

    public static string UpperCaseFirstChar(this string any, bool restToLower = false) => char.ToUpper(any[0]) + (restToLower ? any[1..].ToLower() : any[1..]);
    
    public static string RemoveAllSpecialChars(this string any) => Regex.Replace(any, "[^a-zA-Z0-9]+", string.Empty, RegexOptions.Compiled);
    
    public static string GenerateRandomString(int length = Constants.RandomStringDefaultLength, bool caseSensitive = false, bool includeSpecialChars = false) {
        const string sChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890!@#$%_+.*|";
        const string nChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
        const string scChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890!@#$%_+.*|";
        const string ncChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890";
        
        var charSetToUse = caseSensitive
            ? (includeSpecialChars ? sChars : nChars)
            : (includeSpecialChars ? scChars : ncChars);
        
        var randomString = new string(
            Enumerable.Repeat(charSetToUse, length)
                      .Select(p => p[(new Random()).Next(p.Length)])
                      .ToArray()
        );

        return randomString;
    }
}
