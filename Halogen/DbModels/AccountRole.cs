using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class AccountRole
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public bool IsEffective { get; set; }

    public DateTime? EffectiveUntil { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedById { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account? CreatedBy { get; set; }

    public virtual Role Role { get; set; } = null!;
}
