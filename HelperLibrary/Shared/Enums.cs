namespace HelperLibrary.Shared;
    
public static class Enums {

    public enum RoleType {
        [EnumValue("Customer", 0)]
        Customer,
        [EnumValue("Supplier", 1)]
        Supplier,
        [EnumValue("Admintrator", 2)]
        Administrator,
        [EnumValue("Staff", 3)]
        Staff,
        [EnumValue("Moderator", 4)]
        Moderator
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
        CommaForThousands,
        [EnumValue("{0:#.##0,##}", 1)]
        DotForThousands,
        [EnumValue("{0:#,##}", 2)]
        NoSeparatorCommaForDecimal,
        [EnumValue("{0:#.##}", 3)]
        NoseparatorDotForDecimal
    }

    public enum LocalityRegion {
        [EnumValue("Asia", 0)]
        Asia,
        [EnumValue("Africa", 1)]
        Africa,
        [EnumValue("Europe", 2)]
        Europe,
        [EnumValue("North America", 3)]
        NorthAmerica,
        [EnumValue("Oceania", 4)]
        Oceania,
        [EnumValue("South America", 5)]
        SouthAmerica
    }

    public enum DivisionType {
        State,
        Province
    }

    public enum AddressVariant {
        [EnumValue("PO Box", 1)]
        PoBox,
        [EnumValue("Western", 2)]
        Western,
        [EnumValue("Eastern", 3)]
        Eastern
    }

    public enum ApplicationTheme {
        [EnumValue("Sapphire", 0)]
        Blue,
        [EnumValue("Amber", 1)]
        Orange,
        [EnumValue("Onyx", 2)]
        Carbon,
        [EnumValue("Topaz", 3)]
        Yellow,
        [EnumValue("Emerald", 4)]
        Green,
        [EnumValue("Moonstone", 5)]
        White,
        [EnumValue("Tourmaline", 6)]
        Red,
        [EnumValue("Amethyst", 7)]
        Violet,
        [EnumValue("Aquamarine", 8)]
        Cyan,
        [EnumValue("Alexandrite", 9)]
        Teal,
        [EnumValue("Sodalite", 10)]
        Indigo
    }

    public enum Language {
        [EnumValue("English", 0)]
        English,
        [EnumValue("Vietnamese", 1)]
        Vietnamese,
        [EnumValue("Chinese", 2)]
        Chinese,
        [EnumValue("Japanese", 3)]
        Japanese,
        [EnumValue("French", 4)]
        French,
        [EnumValue("Russian", 5)]
        Russian,
        [EnumValue("Indonesian", 6)]
        Indonesian,
        [EnumValue("Malaysian", 7)]
        Malaysian,
        [EnumValue("Thailand", 8)]
        Thailand,
        [EnumValue("Korean", 9)]
        Korean
    }

    public enum UnitSystem {
        [EnumValue("International Unit System", 0)]
        InternationalUnitSystem,
        [EnumValue("English Unit System", 1)]
        EnglishUnitSystem
    }

    public enum NameFormat {
        [EnumValue("Show full name", 0)]
        ShowFullName,
        [EnumValue("Show first name", 1)]
        ShowFirstName,
        [EnumValue("Show last name", 2)]
        ShowLastName,
        [EnumValue("Show nick name", 3)]
        ShowNickName,
        [EnumValue("Show initials", 4)]
        ShowInitials
    }

    public enum BirthFormat {
        [EnumValue("Show full birthday only", 0)]
        ShowFullBrithOnly,
        [EnumValue("Show full birthday with age", 1)]
        ShowFullBirthAndAge,
        [EnumValue("Show year only", 2)]
        ShowYearOnly,
        [EnumValue("show year with age", 3)]
        ShowYearAndAge,
        [EnumValue("Show day and month only", 4)]
        ShowDayMonthOnly,
        [EnumValue("Show month and year only", 5)]
        ShowMonthYearOnly,
        [EnumValue("Show month and year with age", 6)]
        ShowMonthYearAndAge,
        [EnumValue("Show age only", 7)]
        ShowAgeOnly
    }

    public enum WorkInformationFormat {
        [EnumValue("Show all information", 0)]
        ShowAllInformation,
        [EnumValue("Show company information only", 1)]
        ShowCompanyOnly,
        [EnumValue("Show job title only", 2)]
        ShowJobTitleOnly
    }

    public enum Privacy {
        [EnumValue("Visible to public", 0)]
        VisibleToPublic,
        [EnumValue("Visible to all connections", 1)]
        VisibleToAllConnections,
        [EnumValue("Visible to a group", 2)]
        VisibleToAGroup,
        [EnumValue("Visible to self", 3)]
        VisibleToSelf
    }
}
