using Halogen.Bindings.ApiBindings;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Halogen.Services.DbServices.Services; 

public sealed class ProfileService: DbServiceBase, IProfileService {
    
    public ProfileService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string?> InsertNewProfile(Profile newProfile) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(InsertNewProfile) });
        await _dbContext.Profiles.AddAsync(newProfile);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newProfile.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(InsertNewProfile)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<bool?> IsPhoneNumberAvailableForNewAccount(string phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(IsPhoneNumberAvailableForNewAccount) });
        try {
            var profilePhoneNumbers = await _dbContext.Profiles
                                                      .Where(profile => profile.PhoneNumber != null)
                                                      .Select(profile => JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!)!.ToString())
                                                      .ToArrayAsync();

            return !profilePhoneNumbers.Any(x => x.Equals(phoneNumber));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(IsPhoneNumberAvailableForNewAccount)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<Profile?> GetProfileByAccountId(string accountId) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(GetProfileByAccountId) });
        try {
            return await _dbContext.Profiles.SingleAsync(x => x.AccountId.Equals(accountId));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByAccountId)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByAccountId)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<bool?> UpdateProfile(Profile profile) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(UpdateProfile) });
        _dbContext.Profiles.Update(profile);
        
        try {
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(UpdateProfile)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<Profile?> GetProfileByPhoneNumber(RegionalizedPhoneNumber phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(GetProfileByPhoneNumber) });
        var serializedPhoneNumber = JsonConvert.SerializeObject(phoneNumber);

        try {
            return await _dbContext.Profiles.SingleOrDefaultAsync(x => Equals(x.PhoneNumber, serializedPhoneNumber));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByPhoneNumber)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByPhoneNumber)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }
}