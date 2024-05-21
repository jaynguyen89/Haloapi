using Bogus;
using Halogen.Bindings.ApiBindings;
using Halogen.DbModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace HaloUnitTest.Mocks;

internal static class Fakers {
    private const string SChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890!@$%-_+.*|:<>";
    private const string NChars = "QWERTYUIOPASDFGHJKKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
    private const string ScChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890!@$%-_+.*|:<>";
    private const string NcChars = "QWERTYUIOPASDFGHJKKLZXCVBNM1234567890";

    internal static readonly Faker Faker = new();
    
    internal static readonly Role[] Roles = [
        new Role(Faker.Random.String2(32, NChars), "Customer", false, "Customer"),
        new Role(Faker.Random.String2(32, NChars), "Customer Support", true, "Customer Support"),
        new Role(Faker.Random.String2(32, NChars), "Retail Staff", true, "Retail Staff"),
        new Role(Faker.Random.String2(32, NChars), "Retail Manager", true, "Retail Manager"),
        new Role(Faker.Random.String2(32, NChars), "Warehouse Staff", true, "Warehouse Staff"),
        new Role(Faker.Random.String2(32, NChars), "Warehouse Manager", true, "Warehouse Manager"),
        new Role(Faker.Random.String2(32, NChars), "Accountant", true, "Accountant"),
        new Role(Faker.Random.String2(32, NChars), "Financial Manager", true, "Financial Manager"),
        new Role(Faker.Random.String2(32, NChars), "Marketing Staff", true, "Marketing Staff"),
        new Role(Faker.Random.String2(32, NChars), "Marketing Manager", true, "Marketing Manager"),
        new Role(Faker.Random.String2(32, NChars), "Operation Manager", true, "Operation Manager"),
        new Role(Faker.Random.String2(32, NChars), "Administrator", true, "Administrator"),
    ];

    internal static readonly Currency[] Currencies = [
        new Currency(Faker.Random.String2(32, NChars), "Australian Dollar", "AUD", "$", false),
        new Currency(Faker.Random.String2(32, NChars), "US Dollar", "USD", "$", false),
        new Currency(Faker.Random.String2(32, NChars), "Vietnam Dong", "VNĐ", "Đ", true),
    ];

    internal static readonly Locality[] Localities = [
        new Locality(Faker.Random.String2(32, NChars), "Australia", 0, "au", "aus", "61", Currencies[0].Id, Currencies[1].Id),
        new Locality(Faker.Random.String2(32, NChars), "United States", 0, "us", "usa", "11", Currencies[1].Id, null),
        new Locality(Faker.Random.String2(32, NChars), "Vietnam", 0, "vn", "vnm", "84", Currencies[2].Id, Currencies[2].Id),
    ];
    
    internal sealed class AccountFaker: Faker<Account> {
        public AccountFaker() {
            var username = Faker.Internet.UserName();
            
            RuleFor(e => e.Id, f => f.Random.String2(32, NChars));
            RuleFor(e => e.UniqueCode, Faker.Random.String2(32, NChars));
            RuleFor(e => e.EmailAddress, f => f.Internet.Email());
            RuleFor(e => e.EmailConfirmed, true);
            RuleFor(e => e.Username, username);
            RuleFor(e => e.NormalizedUsername, username.ToUpper());
            RuleFor(e => e.PasswordSalt, f => f.Random.String2(20, 30, SChars));
            RuleFor(e => e.HashPassword, f => f.Random.String2(70, 100, NChars));
        }
    }
    
    internal sealed class AccountRoleFaker: Faker<AccountRole> {
        public AccountRoleFaker(string accountId, string roleId) {
            RuleFor(e => e.Id, Faker.Random.String2(32, NChars));
            RuleFor(e => e.AccountId, accountId);
            RuleFor(e => e.RoleId, roleId);
            RuleFor(e => e.IsEffective, true);
        }
    }
    
    internal sealed class PreferenceFaker: Faker<Preference> {
        public PreferenceFaker(string accountId) {
            RuleFor(e => e.Id, Faker.Random.String2(32, NChars));
            RuleFor(e => e.AccountId, accountId);
            RuleFor(e => e.ApplicationTheme, f => f.Random.Byte(EnumHelpers.Length<Enums.ApplicationTheme>()));
            RuleFor(e => e.ApplicationLanguage, f => f.Random.Byte(EnumHelpers.Length<Enums.Language>()));
            RuleFor(e => e.DateFormat, f => f.Random.Byte(EnumHelpers.Length<Enums.DateFormat>()));
            RuleFor(e => e.TimeFormat, f => f.Random.Byte(EnumHelpers.Length<Enums.TimeFormat>()));
            RuleFor(e => e.NumberFormat, f => f.Random.Byte(EnumHelpers.Length<Enums.NumberFormat>()));
            RuleFor(e => e.UnitSystem, f => f.Random.Byte(EnumHelpers.Length<Enums.UnitSystem>()));
            RuleFor(e => e.Privacy, JsonConvert.SerializeObject(new PrivacyPreferenceFaker().Generate()));
        }
    }

    internal sealed class PrivacyPreferenceFaker: Faker<PrivacyPreference> {
        public PrivacyPreferenceFaker() {
            RuleFor(e => e.ProfilePreference, new ProfilePolicyFaker().Generate());
            RuleFor(e => e.NamePreference, new PrivacyPolicyFaker(nameof(PrivacyPreference.NamePreference)).Generate());
            RuleFor(e => e.BirthPreference, new PrivacyPolicyFaker(nameof(PrivacyPreference.BirthPreference)).Generate());
            RuleFor(e => e.CareerPreference, new PrivacyPolicyFaker(nameof(PrivacyPreference.CareerPreference)).Generate());
            RuleFor(e => e.PhoneNumberPreference, new PrivacyPolicyFaker(nameof(PrivacyPreference.PhoneNumberPreference)).Generate());
            RuleFor(e => e.SecurityPreference, new SecurityPolicyFaker().Generate());
        }

        private sealed class ProfilePolicyFaker: Faker<ProfilePolicy> {
            public ProfilePolicyFaker() {
                RuleFor(e => e.HiddenToSearchEngines, f => f.Random.Bool());
                RuleFor(e => e.HiddenToStrangers, f => f.Random.Bool());
            }
        }

        private sealed class PrivacyPolicyFaker: Faker<PrivacyPolicy> {
            public PrivacyPolicyFaker(string propertyName) {
                RuleFor(e => e.Visibility, f => f.Random.Enum<Enums.Visibility>());
                RuleFor(e => e.DataFormat, f => {
                    var enumMax = propertyName switch {
                        nameof(PrivacyPreference.NamePreference) => EnumHelpers.Length<Enums.NameFormat>(),
                        nameof(PrivacyPreference.BirthPreference) => EnumHelpers.Length<Enums.BirthFormat>(),
                        nameof(PrivacyPreference.CareerPreference) => EnumHelpers.Length<Enums.CareerFormat>(),
                        _ => EnumHelpers.Length<Enums.PhoneNumberFormat>(),
                    };
                    return f.Random.Byte(enumMax);
                });
                // TODO: Add rules to generate VisibleToIds if the Visibility is picked as VisibleToSomeConnections or VisibleToGroups
            }
        }

        private sealed class SecurityPolicyFaker: Faker<SecurityPolicy> {
            public SecurityPolicyFaker() {
                RuleFor(e => e.NotifyLoginIncidentsOnUntrustedDevices, f => f.Random.Bool());
                RuleFor(e => e.PrioritizeLoginNotificationsOverEmail, f => f.Random.Bool());
                RuleFor(e => e.BlockLoginOnUntrustedDevices, f => f.Random.Bool());
            }
        }
    }

    internal sealed class ProfileFaker: Faker<Profile> {
        public ProfileFaker(string accountId) { }
    }
    
    internal sealed class LocalityDivisionFaker: Faker<LocalityDivision> {
        public LocalityDivisionFaker(string localityId) { }
    }
    
    internal sealed class AddressFaker: Faker<Address> {
        public AddressFaker(string[] localityIds, Dictionary<string, string> divisionByLocalityIds) { }
    }
    
    internal sealed class ProfileAddressFaker: Faker<ProfileAddress> {
        public ProfileAddressFaker(string profileId, string addressId) { }
    }
    
    internal sealed class ChallengeFaker: Faker<Challenge> {
        public ChallengeFaker(string[] createdByIds) { }
    }
    
    internal sealed class ChallengeResponseFaker: Faker<ChallengeResponse> {
        public ChallengeResponseFaker(string accountId, string[] challengeIds) { }
    }
    
    internal sealed class TrustedDeviceFaker: Faker<TrustedDevice> {
        public TrustedDeviceFaker(string accountId) { }
    }
}
