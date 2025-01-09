using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services;

public sealed class InterestService: DbServiceBase, IInterestService {
    
    public InterestService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext, haloServiceFactory) { }

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
            return await _dbContext.Interests.Select(interest => new InterestVM {
                Id = interest.Id,
                Name = interest.Name,
                Description = interest.Description,
                Parent = interest.ParentId == null ? null : (InterestVM)interest.Parent!,
            }).ToArrayAsync();
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
}