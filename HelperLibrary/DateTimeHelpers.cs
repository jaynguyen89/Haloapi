namespace HelperLibrary; 

public static class DateTimeHelpers {
    
    public static int? GetAge(this DateTime? dateOfBirth) {
        if (dateOfBirth is null) return default;

        var age = DateTime.UtcNow.Year - dateOfBirth.Value.Year;
        if (dateOfBirth > DateTime.UtcNow.AddYears(age)) age--;
        return age;
    }
}