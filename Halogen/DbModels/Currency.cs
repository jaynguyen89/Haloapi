using System;
using System.Collections.Generic;

namespace Halogen.DbModels
{
    public partial class Currency
    {
        public Currency()
        {
            LocalityPrimaryCurrencies = new HashSet<Locality>();
            LocalitySecondaryCurrencies = new HashSet<Locality>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public bool SymbolPosition { get; set; }

        public virtual ICollection<Locality> LocalityPrimaryCurrencies { get; set; }
        public virtual ICollection<Locality> LocalitySecondaryCurrencies { get; set; }
    }
}
