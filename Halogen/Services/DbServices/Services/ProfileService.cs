﻿using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Controllers;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Halogen.Services.DbServices.Services; 

public sealed class ProfileService: DbServiceBase, IProfileService {
    
    private readonly ICacheService _cacheService;

    public ProfileService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext) {
        var cacheServiceFactory = haloServiceFactory.GetService<CacheServiceFactory>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<AccountController>(nameof(CacheServiceFactory));
        _cacheService = cacheServiceFactory.GetActiveCacheService();
    }

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
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> IsPhoneNumberAvailableForNewAccount(string phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(IsPhoneNumberAvailableForNewAccount) });
        try {
            var profilePhoneNumbers = await _dbContext.Profiles
                                                      .Where(profile => profile.PhoneNumber != null)
                                                      .Select(profile => JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!)!.Simplify())
                                                      .ToArrayAsync();

            return !profilePhoneNumbers.Any(x => x.Equals(phoneNumber));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(IsPhoneNumberAvailableForNewAccount)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Profile?> GetProfileByAccountId(string accountId) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(GetProfileByAccountId) });
        try {
            var profile = await _cacheService.GetCacheEntry<Profile>($"{nameof(Profile)}{Constants.Hyphen}{accountId}")
                          ?? await _dbContext.Profiles.SingleAsync(x => x.AccountId.Equals(accountId));

            await _cacheService.InsertCacheEntry(new MemoryCacheEntry {
                Key = $"{nameof(Profile)}{Constants.Hyphen}{accountId}",
                Value = profile,
                Priority = CacheItemPriority.High,
            });
            
            return profile;
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByAccountId)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByAccountId)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
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
                Severity = Enums.LogSeverity.Error, E = e,
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
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetProfileByPhoneNumber)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Account?> GetAccountByPhoneNumber(RegionalizedPhoneNumber phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(GetAccountByPhoneNumber) });

        try {
            return await _dbContext.Profiles
                .Where(profile => profile.PhoneNumber != null && profile.PhoneNumber.Equals(JsonConvert.SerializeObject(phoneNumber)))
                .Select(profile => profile.Account)
                .SingleAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetAccountByPhoneNumber)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<ProfileService> {
                Location = $"{nameof(GetAccountByPhoneNumber)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}