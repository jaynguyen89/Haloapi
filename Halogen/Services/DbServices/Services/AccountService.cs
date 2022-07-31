using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class AccountService: DbServiceBase, IAccountService {
    
    public AccountService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    public async Task<bool?> IsEmailAddressAvailableForNewAccount(string emailAddress) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(IsEmailAddressAvailableForNewAccount) });
        try {
            return !await _dbContext.Accounts.AnyAsync(x => Equals(x.EmailAddress, emailAddress));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AccountService> { Location = nameof(IsEmailAddressAvailableForNewAccount), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}