using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Halogen.Services.DbServices.Services; 

public sealed class AccountService: DbServiceBase, IAccountService {
    
    public AccountService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger, dbContext, haloServiceFactory) { }
    
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

    public async Task<AuthenticatedUser?> GetInformationForAuthenticatedUser(string accountId) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(GetInformationForAuthenticatedUser) });

        try {
            var authenticatedUser = await _dbContext.Accounts
                .Where(account => account.Id == accountId)
                .Join(
                    _dbContext.Profiles,
                    account => account.Id,
                    profile => profile.AccountId,
                    (account, profile) => new AuthenticatedUser {
                        AccountId = account.Id,
                        ProfileId = profile.Id,
                        Username = account.Username!,
                        FullName = profile.FullName ?? profile.NickName ??
                            $"{profile.GivenName} {profile.MiddleName} {profile.LastName}".Trim(),
                        EmailAddress = account.EmailAddress,
                        PhoneNumber = profile.PhoneNumber == null
                            ? null
                            : JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber),
                    }
                )
                .SingleAsync();

            var roleNames = await _dbContext.AccountRoles
                .Where(accountRole => accountRole.AccountId == accountId && accountRole.IsEffective)
                .Join(
                    _dbContext.Roles,
                    accountRole => accountRole.RoleId,
                    role => role.Id,
                    (accountRole, role) => role.Name
                )
                .ToArrayAsync();

            authenticatedUser.Roles = roleNames.Select(role => role.ToEnum<Enums.Role>() ?? Enums.Role.Customer).ToArray();
            return authenticatedUser;
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetInformationForAuthenticatedUser)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetInformationForAuthenticatedUser)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (OperationCanceledException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetInformationForAuthenticatedUser)}.{nameof(OperationCanceledException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<EmailAddressCredentialVM?> GetEmailAddressCredential(string accountId) {
        _logger.Log(new LoggerBinding<AccountService> { Location = nameof(GetEmailAddressCredential) });

        try {
            return await GetAccountById(accountId) ?? throw new NullReferenceException();
        }
        catch (NullReferenceException e) {
            _logger.Log(new LoggerBinding<AccountService> {
                Location = $"{nameof(GetEmailAddressCredential)}.{nameof(NullReferenceException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}