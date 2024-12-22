using HelperLibrary.Shared.Helpers;

// ReSharper disable once CheckNamespace
namespace Halogen.DbModels; 

public partial class Account {

    public static Account CreateNewAccount(bool useLongerId, string? emailAddress, string salt, string hashedPassword, int verificationTokenLength, string? username) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        UniqueCode = StringHelpers.NewGuid(),
        EmailAddress = emailAddress,
        EmailAddressToken = emailAddress.IsString() ? StringHelpers.GenerateRandomString(verificationTokenLength, true) : default,
        EmailAddressTokenTimestamp = emailAddress.IsString() ? DateTime.UtcNow : default,
        PasswordSalt = salt,
        HashPassword = hashedPassword,
        Username = username,
        NormalizedUsername = username?.ToLower(),
    };
}