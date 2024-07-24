using Halogen.Bindings.ApiBindings;
using HaloUnitTest.Mocks;
using HelperLibrary.Shared.Helpers;
using Moq;

namespace HaloUnitTest.HalogenApiTests.Bindings;

[TestFixture]
public sealed class RegistrationDataTest {

    private RegionalizedPhoneNumberHandler _mockPhoneNumberHandler;

    [SetUp]
    public void Setup() {
        _mockPhoneNumberHandler = It.IsAny<RegionalizedPhoneNumberHandler>();
        
        var authDataMock = MockBase.Simulate<AuthenticationData>();
        authDataMock
            .Setup(m => m.VerifyAuthenticationData(_mockPhoneNumberHandler))
            .ReturnsAsync(new Dictionary<string, List<string>>());
    }

    [Test]
    public async Task Test_VerifyRegistrationData() {
        var registrationData = new RegistrationData {
            EmailAddress = "test@domain.com",
            Password = "Str0ngP@ssword",
            PasswordConfirm = "Str0ngP@ssword",
            Username = "<My_Us3rn@me>",
        };

        var result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        Assert.That(result, Is.Empty);

        registrationData.Username = "";
        result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        var expect = new Dictionary<string, List<string>> {
        {
            nameof(registrationData.Username),
            [$"{nameof(registrationData.Username)} must be provided."]
        }};
        Assert.That(result, Is.EquivalentTo(expect));

        registrationData.Username = "<My_Us3rn@me>";
        registrationData.Password = "";

        result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        expect = new Dictionary<string, List<string>> {
        {
            nameof(registrationData.Password),
            [$"Both {nameof(registrationData.Password)} and {nameof(registrationData.PasswordConfirm).Lucidify()} must be provided."]
        }};
        Assert.That(result, Is.EquivalentTo(expect));

        registrationData.Password = "Str0ngP@ssword";
        registrationData.PasswordConfirm = "";
        
        result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        expect = new Dictionary<string, List<string>> {
        {
            nameof(registrationData.Password),
            [$"Both {nameof(registrationData.Password)} and {nameof(registrationData.PasswordConfirm).Lucidify()} must be provided."]
        }};
        Assert.That(result, Is.EquivalentTo(expect));

        registrationData.Username = "None";
        
        result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        expect = new Dictionary<string, List<string>> {
            {
                nameof(registrationData.Username),
                [$"{nameof(registrationData.Username)} is too short. Min 6, max 65 characters."]
            },
            {
                nameof(registrationData.Password),
                [$"Both {nameof(registrationData.Password)} and {nameof(registrationData.PasswordConfirm).Lucidify()} must be provided."]
            },
        };
        Assert.That(result, Is.EquivalentTo(expect));
        
        registrationData.Username = "<My_Us3rn@me>";
        registrationData.Password = "w3aK";
        registrationData.PasswordConfirm = "@n0ther";
        
        result = await registrationData.VerifyRegistrationData(_mockPhoneNumberHandler);
        expect = new Dictionary<string, List<string>> {
            {
                nameof(registrationData.Password),
                [
                    $"{nameof(registrationData.Password)} length should be at least 8 characters.",
                    $"{nameof(registrationData.Password)} should contain at least 1 special character.",
                    $"{nameof(registrationData.Password)} and {nameof(registrationData.PasswordConfirm).Lucidify()} do not match.",
                ]
            },
        };
        Assert.That(result, Is.EquivalentTo(expect));
    }
}
