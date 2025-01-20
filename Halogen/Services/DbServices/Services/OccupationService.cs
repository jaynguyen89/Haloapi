using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services;

public sealed class OccupationService: DbServiceBase, IOccupationService {
    
    public OccupationService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<OccupationVM[]?> GetAllOccupations() {
        _logger.Log(new LoggerBinding<OccupationService> { Location = nameof(GetAllOccupations) });

        try {
            return await _dbContext.Occupations
                .Select(occupation => new OccupationVM {
                    Id = occupation.Id,
                    Name = occupation.Name,
                    Description = occupation.Description,
                    Parent = occupation.ParentId == null ? null : (OccupationVM)occupation.Parent!,
                })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<OccupationService> {
                Location = $"{nameof(GetAllOccupations)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<OccupationService> {
                Location = $"{nameof(GetAllOccupations)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
    public async Task<OccupationItemVM[]?> GetAllOccupationsAsList() {
        _logger.Log(new LoggerBinding<OccupationService> { Location = nameof(GetAllOccupationsAsList) });

        try {
            return await _dbContext.Occupations
                .Select(occupation => (OccupationItemVM)occupation)
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<OccupationService> {
                Location = $"{nameof(GetAllOccupationsAsList)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<OccupationService> {
                Location = $"{nameof(GetAllOccupationsAsList)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}