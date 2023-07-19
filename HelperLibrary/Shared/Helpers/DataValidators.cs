using System.Text.RegularExpressions;

namespace HelperLibrary.Shared.Helpers; 

public static class DataValidators {
    
    public static List<string> VerifyEmailAddress(this string emailAddress) {
        var errors = new List<string>();
            
        var formatTest = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,4})+)$");
        if (!formatTest.IsMatch(emailAddress)) errors.Add("Email Address format seems to be invalid.");
            
        var lengthTest = new Regex(@".{10,100}");
        if (!lengthTest.IsMatch(emailAddress)) errors.Add($"Email Address is too {emailAddress.ShortOrLong(10, 100)}. Min 10, max 100 characters.");
            
        if (Constants.InvalidTokens.Any(emailAddress.Contains)) errors.Add("Email Address should not contain adjacent special characters.");
        if (Constants.InvalidEnds.Any(emailAddress.StartsWith) || Constants.InvalidEnds.Any(emailAddress.EndsWith))
            errors.Add("Email Address should not start or end with special characters.");

        return errors;
    }

    public static List<string>? VerifyPhoneNumber(this string phoneNumber, int length = 9) =>
        !new Regex($@"^[\d]{length}$").IsMatch(phoneNumber) ? new List<string> { "Phone Number is not in a valid format (eg. 411222333)." } : default;

    public static List<string> VerifyPassword(this string password) {
        var errors = new List<string>();
        
        var lengthTest = new Regex(@".{8,}");
        if (!lengthTest.IsMatch(password)) errors.Add($"{nameof(password).UpperCaseFirstChar()} length should be at least 8 characters.");

        var hasNumberTest = new Regex(@"[\d]+");
        if (!hasNumberTest.IsMatch(password)) errors.Add($"{nameof(password).UpperCaseFirstChar()} should contain at least 1 number.");

        var hasUppercaseCharTest = new Regex(@"[A-Z]+");
        if (!hasUppercaseCharTest.IsMatch(password)) errors.Add($"{nameof(password).UpperCaseFirstChar()} should contain at least 1 uppercase character.");
        
        var hasLowercaseCharTest = new Regex(@"[a-z]+");
        if (!hasLowercaseCharTest.IsMatch(password)) errors.Add($"{nameof(password).UpperCaseFirstChar()} should contain at least 1 lowercase character.");

        var specialChars = Constants.SpecialChars.Select(x => x.First()).ToArray();
        if (password.IndexOfAny(specialChars) == -1) errors.Add($"{nameof(password).UpperCaseFirstChar()} should contain at least 1 special character.");

        if (password.IndexOf(Constants.MonoSpace, StringComparison.Ordinal) != -1) errors.Add($"{nameof(password).UpperCaseFirstChar()} should not contain whitespaces.");
        return errors;
    }
}