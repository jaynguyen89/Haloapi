using HelperLibrary;

namespace Halogen.DbModels; 

public partial class Account {

    internal static Account CreateNewAccount(bool useLongerId, string emailAddress, string salt, string hashedPassword, int verificationTokenLength) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        UniqueIdentifier = StringHelpers.NewGuid(),
        EmailAddress = emailAddress,
        EmailAddressToken = StringHelpers.GenerateRandomString(verificationTokenLength, true),
        EmailAddressTokenTimestamp = DateTime.UtcNow,
        PasswordSalt = salt,
        HashPassword = hashedPassword
    };
}