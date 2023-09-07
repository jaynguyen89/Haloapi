﻿using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class LocalityService: DbServiceBase, ILocalityService {
    
    public LocalityService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string[]?> GetTelephoneCodes() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetTelephoneCodes) });
        try {
            return await _dbContext.Localities.Select(locality => locality.TelephoneCode).ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetTelephoneCodes)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<Locality[]?> GetLocalitiesForPublicData() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetLocalitiesForPublicData) });
        try {
            return await _dbContext.Localities.ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetLocalitiesForPublicData)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }
}
