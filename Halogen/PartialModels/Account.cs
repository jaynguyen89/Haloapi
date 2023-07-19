using HelperLibrary;
using HelperLibrary.Shared.Helpers;

namespace Halogen.DbModels; 

public partial class Account {

    public static Account CreateNewAccount(bool useLongerId, string? emailAddress, string salt, string hashedPassword, int verificationTokenLength) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        UniqueIdentifier = StringHelpers.NewGuid(),
        EmailAddress = emailAddress,
        EmailAddressToken = emailAddress.IsString() ? StringHelpers.GenerateRandomString(verificationTokenLength, true) : default,
        EmailAddressTokenTimestamp = emailAddress.IsString() ? DateTime.UtcNow : default,
        PasswordSalt = salt,
        HashPassword = hashedPassword
    };
}