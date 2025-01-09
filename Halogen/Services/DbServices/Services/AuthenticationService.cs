using Halogen.Auxiliaries.Interfaces;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class AuthenticationService: DbServiceBase, IAuthenticationService {

    public AuthenticationService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext, haloServiceFactory) { }


    public async Task<string?> InsertNewAccount(Account newAccount) {
        _logger.Log(new LoggerBinding<AuthenticationService> { Location = nameof(InsertNewAccount) });
        await _dbContext.Accounts.AddAsync(newAccount);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newAccount.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AuthenticationService> {
                Location = $"{nameof(InsertNewAccount)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}