﻿namespace HelperLibrary.Shared;

public static class Constants {

    public const string ProjectName = "TradeWork EVO";

    public const string Development = "Development";
    public const string Staging = "Staging";
    public const string Production = "Produciton";

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

    public const int RandomStringDefaultLength = 40;

    public static readonly List<string> InvalidEnds = new() { ".", "-", "_" };

    public static readonly List<string> SpecialChars = new()
        { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "-", "+", "=", "{", "[", "}", "]", ":", ";", "<", ",", ">", ".", "?", "|", "~", "`", "/", "\\", "'", "\"" };

    public static readonly List<string> InvalidTokens = new() { "--", "_@", "-@", ".-", "-.", "._", "_.", "@_", "@-", "__", "..", "_-", "-_" };

    public static readonly string[] TelephoneCodes = { "61", "84" };

    public static int TwoFactorDefaultTolerance = 300; // seconds
    
    public static readonly string EmailTemplateFolderPath =
        Path.GetDirectoryName(Directory.GetCurrentDirectory()) +
        @"/AssistantLibrary/EmailTemplates/";
    
    public static readonly Dictionary<string, string> ContentTypes = new() {
        { "json", "application/json" },
        { "form", "multipart/form-data" },
        { "xml", "application/xml" },
        { "mixed", "multipart/mixed" },
        { "alt", "multipart/alternative" },
        { "base64", "application/base64" }
    };
    
    public static readonly List<string> ImageTypes = new() {
        "image/gif", "image/png", "image/jpg", "image/jpeg"
    };
}
