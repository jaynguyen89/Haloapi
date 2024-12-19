using Halogen.Auxiliaries.Interfaces;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;

namespace Halogen.Bindings.ApiBindings;

public sealed class PreferenceUpdateData: ValueData {

    public string FieldName { get; set; } = null!; // Property of PrivacyPreference class
    
    public string PropertyName { get; set; } = null!; // Property of the type of the above property

    public async Task<List<string>?> VerifyData(PreferenceUpdateDataHandler dataHandler) {
        var errors = new List<string>();
        switch (FieldName) {
            case nameof(Preference.ApplicationTheme):
                if (!VerifyByteValue<Enums.ApplicationTheme>(ByteValue))
                    errors.Add($"Value of field \"{FieldName.Lucidify()}\" is not recognized.");
                break;
            case nameof(Preference.ApplicationLanguage):
                if (!VerifyByteValue<Enums.Language>(ByteValue))
                    errors.Add($"Value of field \"{FieldName.Lucidify()}\" is not recognized.");
                break;
            case nameof(Preference.DataFormat):
                switch (PropertyName) {
                    case nameof(DataFormat.DataType.Date):
                        if (!VerifyByteValue<Enums.DateFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(DataFormat.DataType.Time):
                        if (!VerifyByteValue<Enums.TimeFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(DataFormat.DataType.Number):
                        if (!VerifyByteValue<Enums.NumberFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(DataFormat.DataType.UnitSystem):
                        if (!VerifyByteValue<Enums.UnitSystem>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            case nameof(PrivacyPreference.ProfilePreference):
                switch (PropertyName) {
                    case nameof(ProfilePolicy.HiddenToSearchEngines):
                    case nameof(ProfilePolicy.ViewableByStrangers):
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }

                break;
            case nameof(PrivacyPreference.NamePreference):
                switch (PropertyName) {
                    case nameof(PrivacyPolicy.DataFormat):
                        if (!VerifyByteValue<Enums.NameFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.Visibility):
                        if (!VerifyByteValue<Enums.Visibility>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.VisibleToIds):
                        var unmatchedIds = await VerifyVisibleToIds(StrValues!);
                        if (unmatchedIds is null) errors = null;
                        else errors.AddRange(unmatchedIds);
                        
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            case nameof(PrivacyPreference.BirthPreference):
                switch (PropertyName) {
                    case nameof(PrivacyPolicy.DataFormat):
                        if (!VerifyByteValue<Enums.BirthFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.Visibility):
                        if (!VerifyByteValue<Enums.Visibility>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.VisibleToIds):
                        var unmatchedIds = await VerifyVisibleToIds(StrValues!);
                        if (unmatchedIds is null) errors = null;
                        else errors.AddRange(unmatchedIds);
                        
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            case nameof(PrivacyPreference.CareerPreference):
                switch (PropertyName) {
                    case nameof(PrivacyPolicy.DataFormat):
                        if (!VerifyByteValue<Enums.CareerFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.Visibility):
                        if (!VerifyByteValue<Enums.Visibility>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.VisibleToIds):
                        var unmatchedIds = await VerifyVisibleToIds(StrValues!);
                        if (unmatchedIds is null) errors = null;
                        else errors.AddRange(unmatchedIds);
                        
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            case nameof(PrivacyPreference.PhoneNumberPreference):
                switch (PropertyName) {
                    case nameof(PrivacyPolicy.DataFormat):
                        if (!VerifyByteValue<Enums.PhoneNumberFormat>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.Visibility):
                        if (!VerifyByteValue<Enums.Visibility>(ByteValue))
                            errors.Add($"Value of field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        
                        break;
                    case nameof(PrivacyPolicy.VisibleToIds):
                        var unmatchedIds = await VerifyVisibleToIds(StrValues!);
                        if (unmatchedIds is null) errors = null;
                        else errors.AddRange(unmatchedIds);
                        
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            case nameof(PrivacyPreference.SecurityPreference):
                switch (PropertyName) {
                    case nameof(SecurityPolicy.NotifyLoginIncidentsOnUntrustedDevices):
                    case nameof(SecurityPolicy.PrioritizeLoginNotificationsOverEmail):
                    case nameof(SecurityPolicy.BlockLoginOnUntrustedDevices):
                        break;
                    default:
                        errors.Add($"Field \"{PropertyName.Lucidify()}\" in {FieldName.Lucidify()} is not recognized.");
                        break;
                }
                
                break;
            default:
                errors.Add($"Field \"{FieldName.Lucidify()}\" is not recognized.");
                break;
        }

        return errors;

        bool VerifyByteValue<T>(byte value) where T : Enum => value >= EnumHelpers.Length<T>();

        async Task<string[]?> VerifyVisibleToIds(string[] ids) {
            var verification = await dataHandler.AreIdsAssociatedWithAccounts(ids);
            if (verification is null) return null;
            
            var (isAssociated, unmatchedIds) = verification;
            return !isAssociated ? unmatchedIds : [];
        }
    }
}

public sealed class PreferenceUpdateDataHandler {

    private readonly ILoggerService _logger;
    private readonly IAccountService _accountService;

    public PreferenceUpdateDataHandler(
        ILoggerService logger,
        IHaloServiceFactory haloServiceFactory
    ) {
        _logger = logger;
        _accountService = haloServiceFactory.GetService<AccountService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountService>(nameof(PreferenceService));
    }

    public Task<Tuple<bool, string[]>?> AreIdsAssociatedWithAccounts(string[] ids) {
        throw new NotImplementedException();
    }
}
