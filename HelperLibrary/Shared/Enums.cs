using HelperLibrary.Attributes;
using HelperLibrary.Shared.Helpers;

namespace HelperLibrary.Shared;

public static class Enums {

    public enum LogSeverity {
        [Value("Information")]
        Information,
        [Value("Debugging")]
        Debugging,
        [Value("Error")]
        Error,
        [Value("Caution")]
        Caution, // means something the needs to be refactored or reworked
    }

    public enum ServiceType {
        AppService,
        DbService,
        HostedService,
    }

    public enum ContentType {
        Json,
        Form,
        Xml,
        Mixed,
        Alt,
        Base64,
    }

    public enum SocialMedia {
        [Value("Facebook")]
        Facebook,
        [Value("Google")]
        Google,
        [Value("Twitter")]
        Twitter,
        [Value("Instagram")]
        Instagram,
        [Value("Microsoft")]
        Microsoft,
        [Value("LinkedIn")]
        LinkedIn,
        [Value("Youtube")]
        Youtube,
        [Value("Personal")]
        Personal,
        [Value("Business")]
        Business,
    }

    public enum Gender {
        [Value("Not Specified")]
        NotSpecified,
        [Value("Male")]
        Male,
        [Value("Female")]
        Female,
        [Value("Male (Other)")]
        MaleOther,
        [Value("Female (Other)")]
        FemaleOther,
    }

    public enum TokenDestination {
        Sms,
        Email,
    }

    public enum HashAlgorithm {
        BCrypt,
        Rsa,
        Sha512,
        Md5,
        HmacSha512,
    }

    public enum TimeUnit {
        [Value("Millisecond")]
        Millisecond,
        [Value("Second")]
        Second,
        [Value("Minute")]
        Minute,
        [Value("Hour")]
        Hour,
        [Value("Day")]
        Day,
        [Value("Week")]
        Week,
        [Value("Month")]
        Month,
        [Value("Quarter")]
        Quarter,
        [Value("Year")]
        Year,
    }

    public enum AuthorizationFailure {
        [Value("Internal Server Error")]
        InternalServerError,
        [Value("Invalid User")]
        InvalidUser,
        [Value("Mismatched Bearer Token")]
        MismatchedBearerToken,
        [Value("Mismatched Access Token")]
        MismatchedAccessToken,
        [Value("Authorization Expired")]
        AuthorizationExpired,
        [Value("Invalid Role")]
        InvalidRole,
        [Value("Two-Factor Token Not Found")]
        NoTwoFactorToken,
        [Value("Missing Recaptcha Token")]
        NoRecaptchaToken,
        [Value("Human Verification Failed")]
        RecaptchaNotAHuman,
        [Value("Pre-Authorized: Destination Missing")]
        PreAuthorizeNoPath,
        [Value("Pre-Authorize: Wrong Destination")]
        PreAuthorizeWrongPath,
        [Value("Account-Profile Unassociated")]
        AccountProfileUnassociated,
    }

    public enum Role {
        [Value("Customer")]
        Customer,
        [Value("Supplier")]
        Supplier,
        [Value("Administrator")]
        Administrator,
        [Value("Staff")]
        Staff,
        [Value("Moderator")]
        Moderator,
    }

    public enum DateFormat {
        [Value("dd MMM yyyy")]
        DDMMMYYYY, // default if no preference set
        [Value("ddd, dd MMM yyyy")]
        WDDMMMYYYY,
        [Value("dd/MM/yyyy")]
        DDMMYYYYS,
        [Value("ddd, dd/MM/yyyy")]
        WDDMMYYYYS,
        [Value("dd-MM-yyyy")]
        DDMMYYYYD,
        [Value("ddd, dd-MM-yyyy")]
        WDDMMYYYYD,
        [Value("MM-dd-yyyy")]
        MMDDYYYYD,
        [Value("MM/dd/yyyy")]
        MMDDYYYYS,
        [Value("ddd, MM-dd-yyyy")]
        WMMDDYYYYD,
        [Value("ddd, MM/dd/yyyy")]
        WMMDDYYYYS,
        [Value("yyyy/MM/dd")]
        YYYYMMDD, // client-side ISO format
    }
    
    public enum TimeFormat {
        [Value("hh:mm:ss tt")]
        HHMMSSTTC,
        [Value("hh:mm tt")]
        HHMMTTC, // default if no preference set
        [Value("HH:mm:ss")]
        HHMMSSC, // client-side ISO format
        [Value("HH:mm")]
        HHMMC,
        [Value("hh.mm.ss tt")]
        HHMMTTSSD,
        [Value("hh.mm tt")]
        HHMMTTD,
        [Value("HH.mm.ss")]
        HHMMSSD,
        [Value("HH.mm")]
        HHMMD,
    }

    public enum NumberFormat {
        [Value("{0:#,##0.##}")]
        CommaForThousands,
        [Value("{0:#.##0,##}")]
        DotForThousands,
        [Value("{0:#,##}")]
        NoSeparatorCommaForDecimal,
        [Value("{0:#.##}")]
        NoSeparatorDotForDecimal,
    }

    public enum LocalityRegion {
        [CompositeValue("Asia", "as")]
        Asia,
        [CompositeValue("Africa", "af")]
        Africa,
        [CompositeValue("Europe", "eu")]
        Europe,
        [CompositeValue("North America", "na")]
        NorthAmerica,
        [CompositeValue("Oceania", "oc")]
        Oceania,
        [CompositeValue("South America", "sa")]
        SouthAmerica,
    }

    public enum DivisionType {
        State,
        Province,
    }

    public enum AddressVariant {
        [Value("PO Box")]
        PoBox,
        [Value("Western")]
        Western,
        [Value("Eastern")]
        Eastern,
    }

    public enum ApplicationTheme {
        [Value("Bright Day")]
        Day,
        [Value("Clear Night")]
        Night,
        [Value("Deep Ocean")]
        Ocean,
        [Value("Sweet Dream")]
        Dream,
        [Value("Washing Sky")]
        Sky,
        [Value("Grass Hill")]
        Grass,
        [Value("Sunflower Field")]
        Sunflower,
        [Value("Tangerine Peel")]
        Tangerine,
        [Value("Rose Champagne")]
        Rose,
        [Value("Space Earth")]
        Earth,
    }

    public enum Language {
        [CompositeValue("English", "en")]
        English,
        [CompositeValue("Vietnamese", "vi")]
        Vietnamese,
        [CompositeValue("Chinese", "cn")]
        Chinese,
        [CompositeValue("Japanese", "jp")]
        Japanese,
        [CompositeValue("French", "fr")]
        French,
        [CompositeValue("Russian", "rs")]
        Russian,
        [CompositeValue("Indonesian", "in")]
        Indonesian,
        [CompositeValue("Malaysian", "ml")]
        Malaysian,
        [CompositeValue("Thailand", "th")]
        Thailand,
        [CompositeValue("Korean", "kr")]
        Korean,
    }

    public enum UnitSystem {
        [Value("International Unit System")]
        InternationalUnitSystem,
        [Value("English Unit System")]
        EnglishUnitSystem,
    }

    public enum NameFormat {
        [Value("Show full name")]
        ShowFullName,
        [Value("Show first name")]
        ShowFirstName,
        [Value("Show last name")]
        ShowLastName,
        [Value("Show nick name")]
        ShowNickName,
        [Value("Show initials only")]
        ShowInitials,
        [Value("Show initials and nickname")]
        InitialsAndNickName,
    }

    public enum BirthFormat {
        [Value("Show full birthday only")]
        ShowFullBrithOnly,
        [Value("Show full birthday with age")]
        ShowFullBirthAndAge,
        [Value("Show year only")]
        ShowYearOnly,
        [Value("show year with age")]
        ShowYearAndAge,
        [Value("Show day and month only")]
        ShowDayMonthOnly,
        [Value("Show month and year only")]
        ShowMonthYearOnly,
        [Value("Show month and year with age")]
        ShowMonthYearAndAge,
        [Value("Show age only")]
        ShowAgeOnly,
    }

    public enum Ethnicity {
        [Value("Not Specified")]
        NotSpecified,
        [Value("East Asian")]
        EastAsian,
        [Value("North Asian")]
        NorthAsian,
        [Value("South Asian")]
        SouthAsian,
        [Value("South-East Asian")]
        SouthEastAsian,
        [Value("West Asian")]
        WestAsian,
        [Value("Central Asian")]
        CentralAsian,
        [Value("Afro-Asiatic")]
        AfroAsiatic,
        [Value("Niger-Congo")]
        NigerCongo,
        [Value("Nilo-Saharan")]
        NiloSaharan,
        [Value("Khoisan")]
        Khoisan,
        [Value("Austronesian")]
        Austronesian,
        [Value("Indo-European")]
        IndoEuropean,
        [Value("European")]
        European,
        [Value("North American")]
        NorthAmerican,
        [Value("Central American")]
        CentralAmerican,
        [Value("South American")]
        SouthAmerican,
        [Value("Caribbean")]
        Caribbean,
        [Value("Oceanian")]
        Oceanian,
    }

    public enum CareerFormat {
        [Value("Show all information")]
        ShowAllInformation,
        [Value("Show company information only")]
        ShowCompanyOnly,
        [Value("Show job title only")]
        ShowJobTitleOnly,
    }

    public enum PhoneNumberFormat {
        [Value("Ex: (+12)412 345 678")]
        WithRegionCode_SpaceDelimited,
        [Value("Ex: (+12)412.345.678")]
        WithRegionCode_DotDelimited,
        [Value("Ex: (+12)-412-345-678")]
        WithRegionCode_HyphenDelimited,
        [Value("Ex: +12412345678")]
        WithRegionCode_Plain,
        [Value("Ex: 0412 345 678")]
        NoRegionCode_SpaceDelimited,
        [Value("Ex: 0412.345.678")]
        NoRegionCode_DotDelimited,
        [Value("Ex: 0412-345-678")]
        NoRegionCode_HyphenDelimited,
        [Value("Ex: 0412345678")]
        NoRegionCode_Plain,
    }

    public enum Visibility {
        [Value("Visible to public")]
        VisibleToPublic,
        [Value("Visible to all connections")]
        VisibleToAllConnections,
        [Value("Visible to some connections")]
        VisibleToSomeConnections,
        [Value("Visible to some groups of connections")]
        VisibleToGroupsOfConnection,
        [Value("Visible to self")]
        VisibleToSelf,
    }

    public enum DeviceType {
        [Value("Smart phones or tables and other handheld devices")]
        Mobile,
        [Value("Laptops, desktops, mini-PC, compusticks")]
        Computer,
        [Value("Smart TV, automobile head-unit, and streaming or casting devices")]
        Electronic,
        [Value("Other devices that support web browsing or app stores")]
        Other,
    }
    
    public enum OS {
        Android,
        iOS,
        MacOS,
        Windows,
        Linux,
        Other,
    }

    public enum EmailTemplate {
        [Value("AccountActivationEmail")]
        AccountActivationEmail,
        [Value("AccountRecoveryEmail")]
        AccountRecoveryEmail,
        [Value("SecretCodeEmail")]
        SecretCodeEmail,
        [Value("OneTimePasswordEmail")]
        OneTimePasswordEmail,
        [Value("EmailAddressConfirmationEmail")]
        EmailAddressConfirmationEmail,
        [Value("SecurityQuestionsChangedNotification")]
        SecurityQuestionsChangedNotification,
    }

    public enum SessionKey {
        [Value(nameof(Authorization))]
        Authorization,
        [Value(nameof(PreAuthorization))]
        PreAuthorization,
        [Value(nameof(AuthenticatedUser))]
        AuthenticatedUser,
        [Value(nameof(Preference))]
        Preference,
    }
}
