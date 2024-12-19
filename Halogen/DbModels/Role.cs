using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsForStaff { get; set; }

    public string? CreatedById { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual Account? CreatedBy { get; set; }
}
