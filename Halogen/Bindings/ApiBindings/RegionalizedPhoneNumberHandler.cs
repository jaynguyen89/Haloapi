using System.Text.RegularExpressions;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace Halogen.Bindings.ApiBindings;

public sealed class RegionalizedPhoneNumber {
    
    public string RegionCode { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;

    public override string ToString() => $"{Constants.Plus}{RegionCode}{Constants.MonoSpace}{PhoneNumber}";
    
    public string Simplify() => $"{RegionCode}{Constants.Comma}{PhoneNumber}";

    public string ToPhoneNumber() => $"{Constants.Plus}{RegionCode}{PhoneNumber}";

    public static RegionalizedPhoneNumber Convert(string simplifiedPhoneNumber) {
        var tokens = simplifiedPhoneNumber.Split(Constants.Comma);
        return new RegionalizedPhoneNumber {
            RegionCode = tokens[0],
            PhoneNumber = tokens[1],
        };
    }

    public static RegionalizedPhoneNumber? Deserialize(string serializedPhoneNumber) => JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(serializedPhoneNumber);
}

public sealed class RegionalizedPhoneNumberHandler {

    private readonly ICacheService _cacheService;
    
    private readonly ILocalityService _localityService;
    
    private readonly string _cacheKey;

    public RegionalizedPhoneNumberHandler(
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory
    ) {
        _cacheKey = configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys.TelephoneCodes)}");
        
        var cacheService = haloServiceFactory.GetService<RedisCache>(Enums.ServiceType.AppService);
        var localityService = haloServiceFactory.GetService<LocalityService>(Enums.ServiceType.DbService);

        _cacheService = cacheService ?? throw new HaloArgumentNullException<RegionalizedPhoneNumberHandler>(nameof(cacheService));
        _localityService = localityService ?? throw new HaloArgumentNullException<RegionalizedPhoneNumberHandler>(nameof(localityService));
    }

    public async Task<string[]> VerifyPhoneNumberData(RegionalizedPhoneNumber phoneNumber) {
        var errors = new List<string>();
        var telephoneCodes = await _cacheService.GetCacheEntry<string[]>(_cacheKey) ?? await _localityService.GetTelephoneCodes() ?? Constants.TelephoneCodes;

        phoneNumber.RegionCode = Regex.Replace(phoneNumber.RegionCode.Trim(), Constants.MultiSpace, string.Empty);
        phoneNumber.PhoneNumber = Regex.Replace(phoneNumber.PhoneNumber.Trim(), Constants.MultiSpace, string.Empty);
        
        if (!telephoneCodes.Any(x => x.Equals(phoneNumber.RegionCode))) errors.Add($"{nameof(RegionalizedPhoneNumber.RegionCode).Lucidify()} is not found or haven't been supported yet.");
        errors = errors.Concat(phoneNumber.PhoneNumber.VerifyPhoneNumber() ?? new List<string>()).ToList();

        return errors.ToArray();
    }
}