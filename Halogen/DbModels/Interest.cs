using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Interest
{
    public string Id { get; set; } = null!;

    public string? ParentId { get; set; }

    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }

    public virtual ICollection<Interest> InverseParent { get; set; } = new List<Interest>();

    public virtual Interest? Parent { get; set; }
}
