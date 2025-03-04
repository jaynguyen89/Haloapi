﻿namespace HelperLibrary.Shared;

public static class Constants {

    public const string ProjectName = "TradeWork EVO";

    public const string Local = "Local";
    public const string Development = "Development";
    public const string Staging = "Staging";
    public const string Production = "Production";

    public const string MonoSpace = " ";
    public const string MultiSpace = @"\s+";
    public const string FSlash = "/";
    public const string BSlash = "\\";
    public const string Semicolon = ";";
    public const string Colon = ":";
    public const string Comma = ",";
    public const string DoubleQuote = "\"";
    public const string SingleQuote = "'";
    public const string Plus = "+";
    public const string Underscore = "_";
    public const string Hyphen = "-";

    public const int TicksPerSecond = 1000;
    public const int SecondsPerMinute = 60;
    public const int MinutesPerHour = 60;
    public const int HoursPerDay = 24;
    public const int DaysPerWeek = 7;
    public const int DaysPerMonth = 30;
    public const int MonthsPerYear = 12;
    public const double DaysPerYear = 365.24235;

    public const int RandomStringDefaultLength = 40;
    public const int SecretCodeLength = 8;

    public static readonly List<string> InvalidEnds = new() { ".", "-", "_" };

    public static readonly List<string> SpecialChars = new()
        { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "-", "+", "=", "{", "[", "}", "]", ":", ";", "<", ",", ">", ".", "?", "|", "~", "`", "/", "\\", "'", "\"" };

    public static readonly List<string> InvalidTokens = new() { "--", "_@", "-@", ".-", "-.", "._", "_.", "@_", "@-", "__", "..", "_-", "-_" };

    public static readonly string[] TelephoneCodes = { "61", "84" };
    
    public static readonly string AssetsDirectoryPath = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + "/AssistantLibrary/Assets/";
    
    public static readonly Dictionary<string, string> ContentTypes = new() {
        { "json", "application/json" },
        { "form", "multipart/form-data" },
        { "xml", "application/xml" },
        { "mixed", "multipart/mixed" },
        { "alt", "multipart/alternative" },
        { "base64", "application/base64" }
    };

    public const int ImageSize = 3000000;
    public static readonly List<string> ImageTypes = new() {
        "image/gif", "image/png", "image/jpg", "image/jpeg"
    };
}
