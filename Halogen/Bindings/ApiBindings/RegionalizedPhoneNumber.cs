using System.Text.RegularExpressions;
using Halogen.Parsers;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

public sealed class RegionalizedPhoneNumber {

    private readonly ICacheService _cacheService;
    
    private readonly ILocalityService _localityService;
    
    private readonly string _cacheKey;

    public string RegionCode { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;

    public RegionalizedPhoneNumber(
        IConfiguration configuration,
        ICacheService cacheService,
        ILocalityService localityService
    ) {
        _cacheService = cacheService;
        _localityService = localityService;
        _cacheKey = configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys.TelephoneCodes)}");
    }

    public async Task<string[]> VerifyPhoneNumberData() {
        var errors = new List<string>();
        var telephoneCodes = await _cacheService.GetCacheEntry<string[]>(_cacheKey) ?? await _localityService.GetTelephoneCodes() ?? Constants.TelephoneCodes;

        RegionCode = Regex.Replace(RegionCode.Trim(), Constants.MultiSpace, string.Empty);
        PhoneNumber = Regex.Replace(PhoneNumber.Trim(), Constants.MultiSpace, string.Empty);
        
        if (!telephoneCodes.Any(x => x.Equals(RegionCode))) errors.Add($"{nameof(RegionCode).ToHumanStyled()} is not found or haven't been supported yet.");
        errors = errors.Concat(PhoneNumber.VerifyPhoneNumber() ?? new List<string>()).ToList();

        return errors.ToArray();
    }
}