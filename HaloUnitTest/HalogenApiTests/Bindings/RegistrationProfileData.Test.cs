using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HalogenApiTests.Bindings;

[TestFixture]
public class RegistrationProfileDataTest {

    [Test]
    public void Test_VerifyRegistrationProfileData() {
        var profileData = new RegistrationProfileData {
            Gender = 99,
            GivenName = "  !nvalid",
            MiddleName = "inval1d ",
            FamilyName = "  Valid   ",
        };

        var result = profileData.VerifyRegistrationProfileData();
        var expect = new Dictionary<string, List<string>> {
            {nameof(profileData.Gender), [$"{nameof(Enums.Gender)} is not recognized."]},
            {nameof(profileData.GivenName), [$"{nameof(profileData.GivenName).Lucidify()} should only contain alphabetical letters, dots, hyphens and single quotes."]},
            {nameof(profileData.MiddleName), [$"{nameof(profileData.MiddleName).Lucidify()} should only contain alphabetical letters, dots, hyphens and single quotes."]},
        };
        Assert.That(result, Is.EquivalentTo(expect));

        profileData.Gender = 0;
        profileData.GivenName = "Valid's";
        profileData.MiddleName = "O.";
        profileData.FamilyName = "F-Name";

        result = profileData.VerifyRegistrationProfileData();
        Assert.That(result, Is.Empty);
    }
}