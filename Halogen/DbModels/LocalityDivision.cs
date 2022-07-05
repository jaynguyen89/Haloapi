using System;
using System.Collections.Generic;

namespace Halogen.DbModels
{
    public partial class LocalityDivision
    {
        public LocalityDivision()
        {
            Addresses = new HashSet<Address>();
        }

        public string Id { get; set; } = null!;
        public string LocalityId { get; set; } = null!;
        public byte DivisionType { get; set; }
        public string Name { get; set; } = null!;
        public string? Abbreviation { get; set; }

        public virtual Locality Locality { get; set; } = null!;
        public virtual ICollection<Address> Addresses { get; set; }
    }
}
