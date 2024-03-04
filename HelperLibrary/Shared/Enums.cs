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

    public enum GenderType {
        [Value("Male")]
        Male,
        [Value("Female")]
        Female,
        [Value("Male (Other)")]
        MaleOther,
        [Value("Female (Other)")]
        FemaleOther,
        [Value("Not Specified")]
        NotSpecified,
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
        [Value("Invalid User")]
        InvalidUser,
        [Value("Mismatched Bearer Token")]
        MismatchedBearerToken,
        [Value("Mismatched Authentication Token")]
        MismatchedAuthToken,
        [Value("Authorization Expired")]
        AuthorizationExpired,
        [Value("Invalid Role")]
        InvalidRole,
        [Value("Missing Two-Factor Token")]
        NoTwoFactorToken,
        [Value("Mismatched Two-Factor Token")]
        InvalidTwoFactorToken,
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
        DDMMMYYYY,
        [Value("ddd, dd MMM yyyy")]
        WDDMMMYYYY,
        [Value("dd/MM/yyyy")]
        DDMMYYYYS,
        [Value("ddd, dd/MM/YYYY")]
        WDDMMYYYYS,
        [Value("dd-MM-yyyy")]
        DDMMYYYYD,
        [Value("ddd, dd-MM-YYYY")]
        WDDMMYYYYD,
        [Value("yyyy/MM/dd")]
        YYYYMMDDS,
        [Value("yyyy-MM-dd")]
        YYYYMMDDD,
    }
    
    public enum TimeFormat {
        [Value("hh:mm tt")]
        HHMMTTC,
        [Value("HH:mm")]
        HHMMC,
        [Value("hh.mm tt")]
        HHMMTTD,
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
        [Value("Show initials")]
        ShowInitials,
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

    public enum CareerFormat {
        [Value("Show all information")]
        ShowAllInformation,
        [Value("Show company information only")]
        ShowCompanyOnly,
        [Value("Show job title only")]
        ShowJobTitleOnly,
    }

    public enum Visibility {
        [Value("Visible to public")]
        VisibleToPublic,
        [Value("Visible to all connections")]
        VisibleToAllConnections,
        [Value("Visible to a group")]
        VisibleToAGroup,
        [Value("Visible to self")]
        VisibleToSelf,
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
    }
}
