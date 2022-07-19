using System;
using System.Collections.Generic;

namespace Halogen.DbModels
{
    public partial class Locality
    {
        public Locality()
        {
            Addresses = new HashSet<Address>();
            LocalityDivisions = new HashSet<LocalityDivision>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public byte Region { get; set; }
        public string Code { get; set; } = null!;
        public string TelephoneCode { get; set; } = null!;
        public string PrimaryCurrencyId { get; set; } = null!;
        public string? SecondaryCurrencyId { get; set; }

        public virtual Currency PrimaryCurrency { get; set; } = null!;
        public virtual Currency? SecondaryCurrency { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<LocalityDivision> LocalityDivisions { get; set; }
    }
}
