using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

internal interface IRoleService {

    Task<string?> InsertNewAccountRole(AccountRole newAccountRole);
    
    Task<Role?> GetRoleByName(string roleName);
}