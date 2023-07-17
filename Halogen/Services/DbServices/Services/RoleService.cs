using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class RoleService: DbServiceBase, IRoleService {
    
    public RoleService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string?> InsertNewAccountRole(AccountRole newAccountRole) {
        _logger.Log(new LoggerBinding<RoleService> { Location = nameof(InsertNewAccountRole) });
        await _dbContext.AccountRoles.AddAsync(newAccountRole);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newAccountRole.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<RoleService> {
                Location = $"{nameof(InsertNewAccountRole)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<Role?> GetRoleByName(string? roleName) {
        _logger.Log(new LoggerBinding<RoleService> { Location = nameof(GetRoleByName) });
        try {
            return await _dbContext.Roles.SingleAsync(x => x.Name.Equals(roleName));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<RoleService> {
                Location = $"{nameof(GetRoleByName)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<RoleService> {
                Location = $"{nameof(GetRoleByName)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }

    public async Task<Enums.Role[]?> GetAllAccountRoles(string accountId) {
        _logger.Log(new LoggerBinding<RoleService> { Location = nameof(GetAllAccountRoles) });
        try {
            return await _dbContext.AccountRoles
                                   .Where(x => x.AccountId.Equals(accountId))
                                   .Select(x => x.Role.Name)
                                   .Select(x => x.ToEnum(Enums.Role.Customer))
                                   .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<RoleService> {
                Location = $"{nameof(GetAllAccountRoles)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }
}