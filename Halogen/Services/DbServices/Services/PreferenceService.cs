using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Halogen.Services.DbServices.Services; 

public sealed class PreferenceService: DbServiceBase, IPreferenceService {
    
    public PreferenceService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext, haloServiceFactory) { }

    public async Task<string?> InsertNewPreference(Preference newPreference) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(InsertNewPreference) });
        await _dbContext.Preferences.AddAsync(newPreference);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newPreference.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(InsertNewPreference)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<PreferenceVM?> GetPreferenceSettings(string accountId) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(GetPreferenceSettings) });

        try {
            return await GetPreference(accountId) ?? throw new NullReferenceException();
        }
        catch (NullReferenceException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPreferenceSettings)}.{nameof(NullReferenceException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<PrivacyVM?> GetPrivacySettings(string accountId) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(GetPrivacySettings) });

        try {
            var preference = await GetPreference(accountId) ?? throw new NullReferenceException();
            var privacySettings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy) ?? throw new NullReferenceException();
            
            var privacySettingsVm = new PrivacyVM {
                ProfilePreference = privacySettings.ProfilePreference
            };

            var privacyVmProps = new List<string> {
                nameof(PrivacyVM.NamePreference),
                nameof(PrivacyVM.BirthPreference),
                nameof(PrivacyVM.CareerPreference),
                nameof(PrivacyVM.PhoneNumberPreference),
            };

            foreach (var propertyName in privacyVmProps) {
                if (privacySettingsVm is null) throw new NullReferenceException();
                privacySettingsVm = await SetPrivacySettingsVmProperties(privacySettings, privacySettingsVm, propertyName);
            }

            if (privacySettingsVm is null) throw new NullReferenceException();
            privacySettingsVm = await SetPrivacySettingsVmSecurityPreference(privacySettings, privacySettingsVm, accountId);
            
            return privacySettingsVm;
        }
        catch (NullReferenceException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPrivacySettings)}.{nameof(NullReferenceException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Preference?> GetPreference(string accountId) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(GetPreference) });

        try {
            return await _dbContext.Preferences.SingleAsync(preference => preference.AccountId == accountId);
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPreference)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPreference)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> UpdatePreference(Preference preference) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(UpdatePreference) });

        try {
            _dbContext.Preferences.Update(preference);
            return await _dbContext.SaveChangesAsync() != 0;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(UpdatePreference)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(UpdatePreference)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    private async Task<PrivacyVM?> SetPrivacySettingsVmProperties(PrivacyPreference privacySettings, PrivacyVM privacySettingsVm, string propertyName) {
        _logger.Log(new LoggerBinding<PreferenceService> { IsPrivate = true, Location = nameof(SetPrivacySettingsVmProperties) });

        try {
            var visibleToIds = propertyName switch {
                nameof(PrivacyVM.NamePreference) => privacySettings.NamePreference.VisibleToIds,
                nameof(PrivacyVM.BirthPreference) => privacySettings.BirthPreference.VisibleToIds,
                nameof(PrivacyVM.CareerPreference) => privacySettings.CareerPreference.VisibleToIds,
                _ => privacySettings.PhoneNumberPreference.VisibleToIds,
            };
            
            var visibleTos = visibleToIds == null
                ? null
                : await _dbContext.Accounts
                    .Where(account => visibleToIds.Contains(account.Id))
                    .Join(
                        _dbContext.Profiles,
                        account => account.Id,
                        profile => profile.AccountId,
                        (account, profile) => new PrivacyPolicyVM.VisibleToVM {
                            Id = account.Id,
                            Username = account.Username!,
                            Name = profile.GetName(),
                        })
                    .ToArrayAsync();

            privacySettingsVm.NamePreference = new PrivacyPolicyVM {
                DataFormat = propertyName switch {
                    nameof(PrivacyVM.NamePreference) => privacySettings.NamePreference.DataFormat,
                    nameof(PrivacyVM.BirthPreference) => privacySettings.BirthPreference.DataFormat,
                    nameof(PrivacyVM.CareerPreference) => privacySettings.CareerPreference.DataFormat,
                    _ => privacySettings.PhoneNumberPreference.DataFormat,
                },
                Visibility = propertyName switch {
                    nameof(PrivacyVM.NamePreference) => privacySettings.NamePreference.Visibility,
                    nameof(PrivacyVM.BirthPreference) => privacySettings.BirthPreference.Visibility,
                    nameof(PrivacyVM.CareerPreference) => privacySettings.CareerPreference.Visibility,
                    _ => privacySettings.PhoneNumberPreference.Visibility,
                },
                VisibleTos = visibleTos,
            };
            
            return privacySettingsVm;
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(SetPrivacySettingsVmProperties)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    private async Task<PrivacyVM?> SetPrivacySettingsVmSecurityPreference(PrivacyPreference privacySettings, PrivacyVM privacySettingsVm, string accountId) {
        _logger.Log(new LoggerBinding<PreferenceService> { IsPrivate = true, Location = nameof(SetPrivacySettingsVmSecurityPreference) });
        
        privacySettingsVm.SecurityPreference = new SecurityPolicyVM {
            NotifyLoginIncidentsOnUntrustedDevices = privacySettings.SecurityPreference.NotifyLoginIncidentsOnUntrustedDevices,
            NotifyLoginIncidentsOverEmail = privacySettingsVm.SecurityPreference.NotifyLoginIncidentsOverEmail,
            BlockLoginOnUntrustedDevices = privacySettingsVm.SecurityPreference.BlockLoginOnUntrustedDevices,
        };

        if (privacySettingsVm.SecurityPreference.NotifyLoginIncidentsOverEmail) {
            var profile = await _dbContext.Profiles.SingleAsync(profile => profile.AccountId == accountId);
            if (profile is null) throw new NullReferenceException();
            
            privacySettingsVm.SecurityPreference.CanChangeNotifyLoginIncidentsOverEmail = profile.PhoneNumber is not null && profile.PhoneNumberConfirmed;
        }
        else {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account is null) throw new NullReferenceException();
            
            privacySettingsVm.SecurityPreference.CanChangeNotifyLoginIncidentsOverEmail = account.EmailAddress is not null && account.EmailConfirmed;
        }

        if (privacySettingsVm.SecurityPreference.BlockLoginOnUntrustedDevices) privacySettingsVm.SecurityPreference.CanChangeBlockLoginOnUntrustedDevices = true;
        else privacySettingsVm.SecurityPreference.CanChangeBlockLoginOnUntrustedDevices = await _dbContext.TrustedDevices.AnyAsync(device => device.IsTrusted && device.AccountId == accountId);

        return privacySettingsVm;
    }
}