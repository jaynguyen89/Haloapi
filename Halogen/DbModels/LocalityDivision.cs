using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class LocalityDivision
{
    public string Id { get; set; } = null!;

    public string LocalityId { get; set; } = null!;

    public byte DivisionType { get; set; }

    public string Name { get; set; } = null!;

    public string? Abbreviation { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual Locality Locality { get; set; } = null!;
}
