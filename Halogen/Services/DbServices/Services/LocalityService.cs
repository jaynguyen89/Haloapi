using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public class LocalityService: DbServiceBase, ILocalityService {

    public LocalityService() { }

    public LocalityService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public virtual async Task<string[]?> GetTelephoneCodes() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetTelephoneCodes) });
        try {
            return await _dbContext.Localities.Select(locality => locality.TelephoneCode).ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetTelephoneCodes)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<CountryVM[]?> GetCountriesAsPublicData() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetCountriesAsPublicData) });
        try {
            return await _dbContext.Localities
                .Select(locality => (CountryVM)locality)
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetCountriesAsPublicData)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public virtual async Task<CountryVM[]?> GetCountries(bool minimal = true) {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetCountries) });
        try {
            return await _dbContext.Localities
                .Select(locality => minimal
                    ? new CountryVM {
                        Id = locality.Id,
                        Name = locality.Name,
                        IsoCode2Char = locality.IsoCode2Char,
                        IsoCode3Char = locality.IsoCode3Char,
                    }
                    : new CountryVM {
                        Id = locality.Id,
                        Name = locality.Name,
                        Region = (Enums.LocalityRegion)locality.Region,
                        TelephoneCode = locality.TelephoneCode,
                        IsoCode2Char = locality.IsoCode2Char,
                        IsoCode3Char = locality.IsoCode3Char,
                        PrimaryCurrencyId = locality.PrimaryCurrencyId,
                        SecondaryCurrencyId = locality.SecondaryCurrencyId,
                    })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetCountries)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Locality?> GetCountryById(string countryId) {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetCountryById) });
        return await _dbContext.Localities.FindAsync(countryId);
    }
    public async Task<LocalityVM[]?> GetLocalities() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetLocalities) });
        try {
            return await _dbContext.Localities
                .Join(
                    _dbContext.LocalityDivisions,
                    locality => locality.Id,
                    division => division.LocalityId,
                    (locality, division) => new {
                        Country = locality,
                        Division = division,
                    }
                )
                .GroupBy(pair => pair.Country)
                .OrderBy(pair => pair.Key.Name)
                .Select(group => new LocalityVM {
                    Country = group.Key,
                    Divisions = group
                        .Select(item => new DivisionVM {
                            Id = item.Division.Id,
                            Name = item.Division.Name,
                        })
                        .ToArray(),
                })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<LocalityService> {
                Location = $"{nameof(GetCountries)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}
