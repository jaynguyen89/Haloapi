namespace HelperLibrary.Shared;

public static class Enums {

    public enum ApiResult {
        SUCCESS,
        FAILED
    }

    public enum LogSeverity {
        [EnumValue("Information", 0)]
        INFORMATION,
        [EnumValue("Debugging", 1)]
        DEBUGGING,
        [EnumValue("Error", 2)]
        ERROR,
        [EnumValue("Caution", 3)]
        CAUTION // means something the needs to be refactored or reworked
    }

    public enum RoleType {
        [EnumValue("Customer", 0)]
        CUSTOMER,
        [EnumValue("Supplier", 1)]
        SUPPLIER,
        [EnumValue("Admintrator", 2)]
        ADMINISTRATOR,
        [EnumValue("Staff", 3)]
        STAFF,
        [EnumValue("Moderator", 4)]
        MODERATOR
    }

    public enum DateFormat {
        [EnumValue("dd MMM yyyy", 0)]
        DDMMMYYYY,
        [EnumValue("ddd, dd MMM yyyy", 1)]
        WDDMMMYYYY,
        [EnumValue("dd/MM/yyyy", 2)]
        DDMMYYYYS,
        [EnumValue("ddd, dd/MM/YYYY", 3)]
        WDDMMYYYYS,
        [EnumValue("dd-MM-yyyy", 4)]
        DDMMYYYYD,
        [EnumValue("ddd, dd-MM-YYYY", 5)]
        WDDMMYYYYD,
        [EnumValue("yyyy/MM/dd", 6)]
        YYYYMMDDS,
        [EnumValue("yyyy-MM-dd", 7)]
        YYYYMMDDD,
    }
    
    public enum TimeFormat {
        [EnumValue("hh:mm tt", 0)]
        HHMMTTC,
        [EnumValue("HH:mm", 1)]
        HHMMC,
        [EnumValue("hh.mm tt", 2)]
        HHMMTTD,
        [EnumValue("HH.mm", 3)]
        HHMMD,
    }

    public enum NumberFormat {
        [EnumValue("{0:#,##0.##}", 0)]
        COMMA_FOR_THOUSANDS,
        [EnumValue("{0:#.##0,##}", 1)]
        DOT_FOR_THOUSANDS,
        [EnumValue("{0:#,##}", 2)]
        NO_SEPARATOR_COMMA_FOR_DECIMAL,
        [EnumValue("{0:#.##}", 3)]
        NOSEPARATOR_DOT_FOR_DECIMAL
    }

    public enum LocalityRegion {
        [EnumValue("Asia", 0)]
        ASIA,
        [EnumValue("Africa", 1)]
        AFRICA,
        [EnumValue("Europe", 2)]
        EUROPE,
        [EnumValue("North America", 3)]
        NORTH_AMERICA,
        [EnumValue("Oceania", 4)]
        OCEANIA,
        [EnumValue("South America", 5)]
        SOUTH_AMERICA
    }

    public enum DivisionType {
        STATE,
        PROVINCE
    }

    public enum AddressVariant {
        [EnumValue("PO Box", 1)]
        PO_BOX,
        [EnumValue("Western", 2)]
        WESTERN,
        [EnumValue("Eastern", 3)]
        EASTERN
    }

    public enum ApplicationTheme {
        [EnumValue("Sapphire", 0)]
        BLUE,
        [EnumValue("Amber", 1)]
        ORANGE,
        [EnumValue("Onyx", 2)]
        CARBON,
        [EnumValue("Topaz", 3)]
        YELLOW,
        [EnumValue("Emerald", 4)]
        GREEN,
        [EnumValue("Moonstone", 5)]
        WHITE,
        [EnumValue("Tourmaline", 6)]
        RED,
        [EnumValue("Amethyst", 7)]
        VIOLET,
        [EnumValue("Aquamarine", 8)]
        CYAN,
        [EnumValue("Alexandrite", 9)]
        TEAL,
        [EnumValue("Sodalite", 10)]
        INDIGO
    }

    public enum Language {
        [EnumValue("English", 0)]
        ENGLISH,
        [EnumValue("Vietnamese", 1)]
        VIETNAMESE,
        [EnumValue("Chinese", 2)]
        CHINESE,
        [EnumValue("Japanese", 3)]
        JAPANESE,
        [EnumValue("French", 4)]
        FRENCH,
        [EnumValue("Russian", 5)]
        RUSSIAN,
        [EnumValue("Indonesian", 6)]
        INDONESIAN,
        [EnumValue("Malaysian", 7)]
        MALAYSIAN,
        [EnumValue("Thailand", 8)]
        THAILAND,
        [EnumValue("Korean", 9)]
        KOREAN
    }

    public enum UnitSystem {
        [EnumValue("International Unit System", 0)]
        INTERNATIONAL_UNIT_SYSTEM,
        [EnumValue("English Unit System", 1)]
        ENGLISH_UNIT_SYSTEM
    }

    public enum NameFormat {
        [EnumValue("Show full name", 0)]
        SHOW_FULL_NAME,
        [EnumValue("Show first name", 1)]
        SHOW_FIRST_NAME,
        [EnumValue("Show last name", 2)]
        SHOW_LAST_NAME,
        [EnumValue("Show nick name", 3)]
        SHOW_NICK_NAME,
        [EnumValue("Show initials", 4)]
        SHOW_INITIALS
    }

    public enum BirthFormat {
        [EnumValue("Show full birthday only", 0)]
        SHOW_FULL_BRITH_ONLY,
        [EnumValue("Show full birthday with age", 1)]
        SHOW_FULL_BIRTH_AND_AGE,
        [EnumValue("Show year only", 2)]
        SHOW_YEAR_ONLY,
        [EnumValue("show year with age", 3)]
        SHOW_YEAR_AND_AGE,
        [EnumValue("Show day and month only", 4)]
        SHOW_DAY_MONTH_ONLY,
        [EnumValue("Show month and year only", 5)]
        SHOW_MONTH_YEAR_ONLY,
        [EnumValue("Show month and year with age", 6)]
        SHOW_MONTH_YEAR_AND_AGE,
        [EnumValue("Show age only", 7)]
        SHOW_AGE_ONLY
    }

    public enum WorkInformationFormat {
        [EnumValue("Show all information", 0)]
        SHOW_ALL_INFORMATION,
        [EnumValue("Show company information only", 1)]
        SHOW_COMPANY_ONLY,
        [EnumValue("Show job title only", 2)]
        SHOW_JOB_TITLE_ONLY
    }

    public enum Privacy {
        [EnumValue("Visible to public", 0)]
        VISIBLE_TO_PUBLIC,
        [EnumValue("Visible to all connections", 1)]
        VISIBLE_TO_ALL_CONNECTIONS,
        [EnumValue("Visible to a group", 2)]
        VISIBLE_TO_A_GROUP,
        [EnumValue("Visible to self", 3)]
        VISIBLE_TO_SELF
    }

    public enum EmailTemplate {
        
    }
}
