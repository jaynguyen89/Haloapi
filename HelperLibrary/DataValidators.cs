using System.Text.RegularExpressions;
using HelperLibrary.Shared;

namespace HelperLibrary; 

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
}