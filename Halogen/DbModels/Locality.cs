using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Locality
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public byte Region { get; set; }

    public string IsoCode2Char { get; set; } = null!;

    public string IsoCode3Char { get; set; } = null!;

    public string TelephoneCode { get; set; } = null!;

    public string PrimaryCurrencyId { get; set; } = null!;

    public string? SecondaryCurrencyId { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<LocalityDivision> LocalityDivisions { get; set; } = new List<LocalityDivision>();

    public virtual Currency PrimaryCurrency { get; set; } = null!;

    public virtual Currency? SecondaryCurrency { get; set; }
}
