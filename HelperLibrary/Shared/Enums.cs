namespace HelperLibrary.Shared;

public static class Enums {

    public enum ApiResult {
        Success = 1,
        Failed = 0
    }

    public enum LogSeverity {
        [Value("Information")]
        Information,
        [Value("Debugging")]
        Debugging,
        [Value("Error")]
        Error,
        [Value("Caution")]
        Caution // means something the needs to be refactored or reworked
    }
    
    public enum TokenDestination {
        Sms,
        Email
    }

    /// <summary>
    /// For the endpoint that forwards token to email or SMS.
    /// No Two-Factor PIN in here as it is supposed to be obtained from Authenticator App.
    /// </summary>
    public enum TokenType {
        EmailRegistration,
        PhoneRegistration,
        OneTimePassword,
        AccountRecovery
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
        Year
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
        InvalidTwoFactorToken
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
        Moderator
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
        NoSeparatorDotForDecimal
    }

    public enum LocalityRegion {
        [CompositeValue("Asia", 0)]
        Asia,
        [CompositeValue("Africa", 1)]
        Africa,
        [CompositeValue("Europe", 2)]
        Europe,
        [CompositeValue("North America", 3)]
        NorthAmerica,
        [CompositeValue("Oceania", 4)]
        Oceania,
        [CompositeValue("South America", 5)]
        SouthAmerica
    }

    public enum DivisionType {
        State,
        Province
    }

    public enum AddressVariant {
        [Value("PO Box")]
        PoBox,
        [Value("Western")]
        Western,
        [Value("Eastern")]
        Eastern
    }

    public enum ApplicationTheme {
        [CompositeValue("Sapphire", 0)]
        Blue,
        [CompositeValue("Amber", 1)]
        Orange,
        [CompositeValue("Onyx", 2)]
        Carbon,
        [CompositeValue("Topaz", 3)]
        Yellow,
        [CompositeValue("Emerald", 4)]
        Green,
        [CompositeValue("Moonstone", 5)]
        White,
        [CompositeValue("Tourmaline", 6)]
        Red,
        [CompositeValue("Amethyst", 7)]
        Violet,
        [CompositeValue("Aquamarine", 8)]
        Cyan,
        [CompositeValue("Alexandrite", 9)]
        Teal,
        [CompositeValue("Sodalite", 10)]
        Indigo
    }

    public enum Language {
        [Value("English")]
        English,
        [Value("Vietnamese")]
        Vietnamese,
        [Value("Chinese")]
        Chinese,
        [Value("Japanese")]
        Japanese,
        [Value("French")]
        French,
        [Value("Russian")]
        Russian,
        [Value("Indonesian")]
        Indonesian,
        [Value("Malaysian")]
        Malaysian,
        [Value("Thailand")]
        Thailand,
        [Value("Korean")]
        Korean
    }

    public enum UnitSystem {
        [Value("International Unit System")]
        InternationalUnitSystem,
        [Value("English Unit System")]
        EnglishUnitSystem
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
        ShowInitials
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
        ShowAgeOnly
    }

    public enum CareerFormat {
        [Value("Show all information")]
        ShowAllInformation,
        [Value("Show company information only")]
        ShowCompanyOnly,
        [Value("Show job title only")]
        ShowJobTitleOnly
    }

    public enum Visibility {
        [Value("Visible to public")]
        VisibleToPublic,
        [Value("Visible to all connections")]
        VisibleToAllConnections,
        [Value("Visible to a group")]
        VisibleToAGroup,
        [Value("Visible to self")]
        VisibleToSelf
    }

    public enum EmailTemplate {
        [Value("AccountActivationEmail")]
        AccountActivationEmail,
        [Value("AccountRecoveryEmail")]
        AccountRecoveryEmail,
        [Value("OneTimePasswordEmail")]
        OneTimePasswordEmail
    }
}
