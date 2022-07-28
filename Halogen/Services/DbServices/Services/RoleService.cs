using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

internal sealed class RoleService: DbServiceBase, IRoleService {
    
    internal RoleService(
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
            _logger.Log(new LoggerBinding<RoleService> { Location = nameof(InsertNewAccountRole), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }

    public async Task<Role?> GetRoleByName(string? roleName) {
        _logger.Log(new LoggerBinding<RoleService> { Location = nameof(GetRoleByName) });
        try {
            return await _dbContext.Roles.SingleAsync(x => x.Name.Equals(roleName));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<RoleService> { Location = nameof(GetRoleByName), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<RoleService> { Location = nameof(GetRoleByName), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}