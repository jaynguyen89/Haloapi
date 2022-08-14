using Halogen.DbModels;
using HelperLibrary.Shared;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IRoleService {

    Task<string?> InsertNewAccountRole(AccountRole newAccountRole);
    
    Task<Role?> GetRoleByName(string roleName);
    
    Task<Enums.Role[]?> GetAllAccountRoles(string accountId);
}