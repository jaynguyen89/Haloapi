// ReSharper disable once CheckNamespace
namespace Halogen.DbModels;

public partial class Role {
    public Role(string id, string name, bool isForStaff, string? description) {
        Id = id;
        Name = name;
        IsForStaff = isForStaff;
        Description = description;
        
        AccountRoles = new HashSet<AccountRole>();
    }
}
