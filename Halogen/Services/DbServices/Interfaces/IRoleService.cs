using Halogen.DbModels;
using HelperLibrary.Shared;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IRoleService {

    /// <summary>
    /// To insert an AccountRole entity.
    /// </summary>
    /// <param name="newAccountRole">AccountRole</param>
    /// <returns>string?</returns>
    Task<string?> InsertNewAccountRole(AccountRole newAccountRole);
    
    /// <summary>
    /// To get a Role entity by its name.
    /// </summary>
    /// <param name="roleName">string</param>
    /// <returns>Role?</returns>
    Task<Role?> GetRoleByName(string roleName);
    
    /// <summary>
    /// To get all Roles as enum from database, associated with an accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>Enums.Role[]?</returns>
    Task<Enums.Role[]?> GetAllAccountRoles(string accountId);
}