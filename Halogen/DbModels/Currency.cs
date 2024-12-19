using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Currency
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public virtual ICollection<Locality> LocalityPrimaryCurrencies { get; set; } = new List<Locality>();

    public virtual ICollection<Locality> LocalitySecondaryCurrencies { get; set; } = new List<Locality>();
}
