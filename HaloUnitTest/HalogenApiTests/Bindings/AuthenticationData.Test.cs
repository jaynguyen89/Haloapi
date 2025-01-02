using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HaloUnitTest.Mocks.HaloApi.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HalogenApiTests.Bindings;

[TestFixture]
public sealed class AuthenticationDataTest {

    private RegionalizedPhoneNumberHandler _mockPhoneNumberHandler;
    
    private readonly string[] _regionCodes = ["61", "84"];
    private const string ConfigKey = $"{nameof(HalogenOptions)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys.TelephoneCodes)}";
    private const string ConfigValue = "mock_cache_key_locality_region_codes";

    [OneTimeSetUp]
    public void Setup() {
        var configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(ConfigKey, ConfigValue));

        var localitySvMock = LocalityServiceMock.Instance(nameof(ILocalityService.GetTelephoneCodes), _regionCodes);
        var cacheSvMock = CacheServiceMock.Instance<string, string[]>(nameof(IRedisCacheService.GetCacheEntry), ConfigValue, _regionCodes);
        
        var haloSvFactoryMock = HaloServiceFactoryMock.Instance<object>([
            new KeyValuePair<Enums.ServiceType, object>(Enums.ServiceType.DbService, localitySvMock),
            new KeyValuePair<Enums.ServiceType, object>(Enums.ServiceType.AppService, cacheSvMock),
        ]);

        _mockPhoneNumberHandler = new RegionalizedPhoneNumberHandler(configMock, haloSvFactoryMock);
    }

    [Test]
    public async Task Test_LoginInformation() {
        var loginInfo = new LoginInformation {
            EmailAddress = "sampl3.email-test@do-ma1n.com.au",
            PhoneNumber = new RegionalizedPhoneNumber {
                RegionCode = "61",
                PhoneNumber = "412357159",
            },
        };

        var errors = await loginInfo.VerifyLoginInformation(_mockPhoneNumberHandler);
        Assert.That(errors.Count, Is.EqualTo(0));

        loginInfo.EmailAddress = "+sampl3..email-test@do-ma1n.com.au";
        loginInfo.PhoneNumber.RegionCode = "123";
        loginInfo.PhoneNumber.PhoneNumber = "4123571590";

        errors = await loginInfo.VerifyLoginInformation(_mockPhoneNumberHandler);

        var expect = new Dictionary<string, List<string>> {
            {
                nameof(loginInfo.EmailAddress),
                [
                    $"{nameof(loginInfo.EmailAddress).Lucidify()} format seems to be invalid.",
                    $"{nameof(loginInfo.EmailAddress).Lucidify()} should not contain adjacent special characters."
                ]
            },
            {
                nameof(loginInfo.PhoneNumber),
                [
                    $"{nameof(loginInfo.PhoneNumber.RegionCode).Lucidify()} hasn't been supported yet.",
                    $"{nameof(loginInfo.PhoneNumber).Lucidify()} is not in a valid format (eg. 411222333)."
                ]
            },
        };
        Assert.That(errors, Is.EquivalentTo(expect));
    }
}
