using Halogen.DbContexts;
using Halogen.DbModels;
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
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(IsEmailAddressAvailableForNewAccount)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Account?> GetAccountById(string accountId) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(GetAccountById) });
        try {
            return await _dbContext.Accounts.FindAsync(accountId);
        }
        catch (Exception e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetAccountById)}.{nameof(Exception)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> UpdateAccount(Account account) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(UpdateAccount) });
        _dbContext.Accounts.Update(account);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(UpdateAccount)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Account?> GetAccountByEmailAddress(string emailAddress) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(GetAccountByEmailAddress) });
        try {
            return await _dbContext.Accounts.SingleOrDefaultAsync(x => Equals(x.EmailAddress, emailAddress));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetAccountByEmailAddress)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetAccountByEmailAddress)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}