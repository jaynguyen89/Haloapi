﻿using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Halogen.Services.DbServices.Services;

public sealed class InterestService: DbServiceBase, IInterestService {
    
    public InterestService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string[]?> GetAllIds() {
        _logger.Log(new LoggerBinding<InterestService> { Location = nameof(GetAllIds) });

        try {
            return await _dbContext.Interests.Select(interest => interest.Id).ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllIds)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllIds)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<InterestVM[]?> GetAllInterests() {
        _logger.Log(new LoggerBinding<InterestService> { Location = nameof(GetAllInterests) });

        try {
            return await _dbContext.Interests
                .Select(interest => new InterestVM {
                    Id = interest.Id,
                    Name = interest.Name,
                    Description = interest.Description,
                    Parent = interest.ParentId == null ? null : (InterestVM)interest.Parent!,
                })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllInterests)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllInterests)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<InterestItemVM[]> GetAllInterestsAsList() {
        _logger.Log(new LoggerBinding<InterestService> { Location = nameof(GetAllInterestsAsList) });

        try {
            return await _dbContext.Interests
                .Select(interest => (InterestItemVM)interest)
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllInterestsAsList)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetAllInterestsAsList)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<InterestVM[]?> GetProfileInterests(string profileId) {
        _logger.Log(new LoggerBinding<InterestService> { Location = nameof(GetProfileInterests) });

        try {
            var profile = await _dbContext.Profiles.FindAsync(profileId);
            if (profile is null) return default;
            if (profile.Interests is null) return [];

            var profileInterestIds = JsonConvert.DeserializeObject<string[]>(profile.Interests);
            var interests = await _dbContext.Interests
                .Where(interest => profileInterestIds!.Contains(interest.Id))
                .Select(interest => (InterestVM)interest)
                .ToArrayAsync();
            
            return interests;
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetProfileInterests)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<InterestService> {
                Location = $"{nameof(GetProfileInterests)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}