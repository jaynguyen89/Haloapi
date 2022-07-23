using System.Text.RegularExpressions;
using Halogen.Parsers;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

internal sealed class PhoneNumberData {

    private readonly ICacheService _cacheService;
    
    private readonly ILocalityService _localityService;
    
    private readonly string _cacheKey;

    public string TelephoneCode { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;

    internal PhoneNumberData(
        IConfiguration configuration,
        ICacheService cacheService,
        ILocalityService localityService
    ) {
        _cacheService = cacheService;
        _localityService = localityService;
        _cacheKey = configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys)}{Constants.Colon}{nameof(HalogenOptions.CacheKeys.TelephoneCodes)}");
    }

    internal async Task<string[]> VerifyPhoneNumberData() {
        var errors = new List<string>();
        var telephoneCodes = await _cacheService.GetCacheEntry<string[]>(_cacheKey) ?? await _localityService.GetTelephoneCodes() ?? Constants.TelephoneCodes;

        TelephoneCode = Regex.Replace(TelephoneCode.Trim(), Constants.MultiSpace, string.Empty);
        PhoneNumber = Regex.Replace(PhoneNumber.Trim(), Constants.MultiSpace, string.Empty);
        
        if (!telephoneCodes.Any(x => x.Equals(TelephoneCode))) errors.Add($"{nameof(TelephoneCode).ToHumanStyled()} is not found or haven't been supported yet.");
        errors = errors.Concat(PhoneNumber.VerifyPhoneNumber() ?? new List<string>()).ToList();

        return errors.ToArray();
    }
}