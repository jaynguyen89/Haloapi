using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Occupation
{
    public string Id { get; set; } = null!;

    public string? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Occupation> InverseParent { get; set; } = new List<Occupation>();

    public virtual Occupation? Parent { get; set; }

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();
}
