using System.Text.RegularExpressions;
using Halogen.FactoriesAndMiddlewares.Interfaces;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings; 

public sealed class RegionalizedPhoneNumber {

    private readonly ICacheService _cacheService;
    
    private readonly ILocalityService _localityService;
    
    private readonly string _cacheKey;

    public string RegionCode { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;

    public RegionalizedPhoneNumber(
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory
    ) {
        _cacheKey = configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys.TelephoneCodes)}");
        
        var cacheService = haloServiceFactory.GetService<RedisCache>(Enums.ServiceType.AppService);
        var localityService = haloServiceFactory.GetService<LocalityService>(Enums.ServiceType.DbService);

        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _localityService = localityService ?? throw new ArgumentNullException(nameof(localityService));
    }

    public async Task<string[]> VerifyPhoneNumberData() {
        var errors = new List<string>();
        var telephoneCodes = await _cacheService.GetCacheEntry<string[]>(_cacheKey) ?? await _localityService.GetTelephoneCodes() ?? Constants.TelephoneCodes;

        RegionCode = Regex.Replace(RegionCode.Trim(), Constants.MultiSpace, string.Empty);
        PhoneNumber = Regex.Replace(PhoneNumber.Trim(), Constants.MultiSpace, string.Empty);
        
        if (!telephoneCodes.Any(x => x.Equals(RegionCode))) errors.Add($"{nameof(RegionCode).Lucidify()} is not found or haven't been supported yet.");
        errors = errors.Concat(PhoneNumber.VerifyPhoneNumber() ?? new List<string>()).ToList();

        return errors.ToArray();
    }

    public override string ToString() => $"{Constants.Plus}{RegionCode}{Constants.MonoSpace}{PhoneNumber}";
    
    public string Lucidify() => $"{Constants.Plus}{RegionCode}{Constants.MonoSpace}{PhoneNumber}";
}