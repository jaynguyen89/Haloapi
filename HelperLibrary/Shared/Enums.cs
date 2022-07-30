namespace HelperLibrary.Shared;

public static class Enums {

    public enum ApiResult {
        SUCCESS = 1,
        FAILED = 0
    }

    public enum LogSeverity {
        [CompositeValue("Information", 0)]
        INFORMATION,
        [CompositeValue("Debugging", 1)]
        DEBUGGING,
        [CompositeValue("Error", 2)]
        ERROR,
        [CompositeValue("Caution", 3)]
        CAUTION // means something the needs to be refactored or reworked
    }

    public enum TimeUnit {
        [Value("Millisecond")]
        MILLISECOND,
        [Value("Second")]
        SECOND,
        [Value("Minute")]
        MINUTE,
        [Value("Hour")]
        HOUR,
        [Value("Day")]
        DAY,
        [Value("Week")]
        WEEK,
        [Value("Month")]
        MONTH,
        [Value("Quarter")]
        QUARTER,
        [Value("Year")]
        YEAR
    }

    public enum AuthorizationFailure {
        [CompositeValue("Invalid User", 0)]
        INVALID_USER,
        [CompositeValue("Mismatched Bearer Token", 1)]
        MISMATCHED_BEARER_TOKEN,
        [CompositeValue("Mismatched Authentication Token", 2)]
        MISMATCHED_AUTH_TOKEN,
        [CompositeValue("Authorization Expired", 3)]
        AUTHORIZATION_EXPIRED,
        [CompositeValue("Invalid Role", 4)]
        INVALID_ROLE,
        [CompositeValue("Missing Two-Factor Token", 5)]
        NO_TWO_FACTOR_TOKEN,
        [CompositeValue("Mismatched Two-Factor Token", 6)]
        INVALID_TWO_FACTOR_TOKEN
    }

    public enum Role {
        [CompositeValue("Customer", 0)]
        CUSTOMER,
        [CompositeValue("Supplier", 1)]
        SUPPLIER,
        [CompositeValue("Administrator", 2)]
        ADMINISTRATOR,
        [CompositeValue("Staff", 3)]
        STAFF,
        [CompositeValue("Moderator", 4)]
        MODERATOR
    }

    public enum DateFormat {
        [CompositeValue("dd MMM yyyy", 0)]
        DDMMMYYYY,
        [CompositeValue("ddd, dd MMM yyyy", 1)]
        WDDMMMYYYY,
        [CompositeValue("dd/MM/yyyy", 2)]
        DDMMYYYYS,
        [CompositeValue("ddd, dd/MM/YYYY", 3)]
        WDDMMYYYYS,
        [CompositeValue("dd-MM-yyyy", 4)]
        DDMMYYYYD,
        [CompositeValue("ddd, dd-MM-YYYY", 5)]
        WDDMMYYYYD,
        [CompositeValue("yyyy/MM/dd", 6)]
        YYYYMMDDS,
        [CompositeValue("yyyy-MM-dd", 7)]
        YYYYMMDDD,
    }
    
    public enum TimeFormat {
        [CompositeValue("hh:mm tt", 0)]
        HHMMTTC,
        [CompositeValue("HH:mm", 1)]
        HHMMC,
        [CompositeValue("hh.mm tt", 2)]
        HHMMTTD,
        [CompositeValue("HH.mm", 3)]
        HHMMD,
    }

    public enum NumberFormat {
        [CompositeValue("{0:#,##0.##}", 0)]
        COMMA_FOR_THOUSANDS,
        [CompositeValue("{0:#.##0,##}", 1)]
        DOT_FOR_THOUSANDS,
        [CompositeValue("{0:#,##}", 2)]
        NO_SEPARATOR_COMMA_FOR_DECIMAL,
        [CompositeValue("{0:#.##}", 3)]
        NOSEPARATOR_DOT_FOR_DECIMAL
    }

    public enum LocalityRegion {
        [CompositeValue("Asia", 0)]
        ASIA,
        [CompositeValue("Africa", 1)]
        AFRICA,
        [CompositeValue("Europe", 2)]
        EUROPE,
        [CompositeValue("North America", 3)]
        NORTH_AMERICA,
        [CompositeValue("Oceania", 4)]
        OCEANIA,
        [CompositeValue("South America", 5)]
        SOUTH_AMERICA
    }

    public enum DivisionType {
        STATE,
        PROVINCE
    }

    public enum AddressVariant {
        [CompositeValue("PO Box", 1)]
        PO_BOX,
        [CompositeValue("Western", 2)]
        WESTERN,
        [CompositeValue("Eastern", 3)]
        EASTERN
    }

    public enum ApplicationTheme {
        [CompositeValue("Sapphire", 0)]
        BLUE,
        [CompositeValue("Amber", 1)]
        ORANGE,
        [CompositeValue("Onyx", 2)]
        CARBON,
        [CompositeValue("Topaz", 3)]
        YELLOW,
        [CompositeValue("Emerald", 4)]
        GREEN,
        [CompositeValue("Moonstone", 5)]
        WHITE,
        [CompositeValue("Tourmaline", 6)]
        RED,
        [CompositeValue("Amethyst", 7)]
        VIOLET,
        [CompositeValue("Aquamarine", 8)]
        CYAN,
        [CompositeValue("Alexandrite", 9)]
        TEAL,
        [CompositeValue("Sodalite", 10)]
        INDIGO
    }

    public enum Language {
        [CompositeValue("English", 0)]
        ENGLISH,
        [CompositeValue("Vietnamese", 1)]
        VIETNAMESE,
        [CompositeValue("Chinese", 2)]
        CHINESE,
        [CompositeValue("Japanese", 3)]
        JAPANESE,
        [CompositeValue("French", 4)]
        FRENCH,
        [CompositeValue("Russian", 5)]
        RUSSIAN,
        [CompositeValue("Indonesian", 6)]
        INDONESIAN,
        [CompositeValue("Malaysian", 7)]
        MALAYSIAN,
        [CompositeValue("Thailand", 8)]
        THAILAND,
        [CompositeValue("Korean", 9)]
        KOREAN
    }

    public enum UnitSystem {
        [CompositeValue("International Unit System", 0)]
        INTERNATIONAL_UNIT_SYSTEM,
        [CompositeValue("English Unit System", 1)]
        ENGLISH_UNIT_SYSTEM
    }

    public enum NameFormat {
        [CompositeValue("Show full name", 0)]
        SHOW_FULL_NAME,
        [CompositeValue("Show first name", 1)]
        SHOW_FIRST_NAME,
        [CompositeValue("Show last name", 2)]
        SHOW_LAST_NAME,
        [CompositeValue("Show nick name", 3)]
        SHOW_NICK_NAME,
        [CompositeValue("Show initials", 4)]
        SHOW_INITIALS
    }

    public enum BirthFormat {
        [CompositeValue("Show full birthday only", 0)]
        SHOW_FULL_BRITH_ONLY,
        [CompositeValue("Show full birthday with age", 1)]
        SHOW_FULL_BIRTH_AND_AGE,
        [CompositeValue("Show year only", 2)]
        SHOW_YEAR_ONLY,
        [CompositeValue("show year with age", 3)]
        SHOW_YEAR_AND_AGE,
        [CompositeValue("Show day and month only", 4)]
        SHOW_DAY_MONTH_ONLY,
        [CompositeValue("Show month and year only", 5)]
        SHOW_MONTH_YEAR_ONLY,
        [CompositeValue("Show month and year with age", 6)]
        SHOW_MONTH_YEAR_AND_AGE,
        [CompositeValue("Show age only", 7)]
        SHOW_AGE_ONLY
    }

    public enum CareerFormat {
        [CompositeValue("Show all information", 0)]
        SHOW_ALL_INFORMATION,
        [CompositeValue("Show company information only", 1)]
        SHOW_COMPANY_ONLY,
        [CompositeValue("Show job title only", 2)]
        SHOW_JOB_TITLE_ONLY
    }

    public enum Visibility {
        [CompositeValue("Visible to public", 0)]
        VISIBLE_TO_PUBLIC,
        [CompositeValue("Visible to all connections", 1)]
        VISIBLE_TO_ALL_CONNECTIONS,
        [CompositeValue("Visible to a group", 2)]
        VISIBLE_TO_A_GROUP,
        [CompositeValue("Visible to self", 3)]
        VISIBLE_TO_SELF
    }

    public enum EmailTemplate {
        [Value("AccountActivationEmail")]
        ACCOUNT_ACTIVATION_EMAIL
    }
}
