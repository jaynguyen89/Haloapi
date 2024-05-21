namespace Halogen.DbModels;

public partial class Role {
    public Role(string id, string name, bool forStaff, string? description) {
        Id = id;
        Name = name;
        IsForStaff = forStaff;
        Description = description;
        
        AccountRoles = new HashSet<AccountRole>();
    }
}
