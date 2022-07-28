using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

internal sealed class AccountService: DbServiceBase, IAccountService {
    
    internal AccountService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    public async Task<bool?> IsEmailAvailableForNewAccount(string emailAddress) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(IsEmailAvailableForNewAccount) });
        try {
            return await _dbContext.Accounts.AnyAsync(x => x.EmailAddress.Equals(emailAddress));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AccountService> { Location = nameof(IsEmailAvailableForNewAccount), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}