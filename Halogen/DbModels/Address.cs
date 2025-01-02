using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Address
{
    public string Id { get; set; } = null!;

    public string? BuildingName { get; set; }

    public string? PoBoxNumber { get; set; }

    public string StreetAddress { get; set; } = null!;

    public string? Group { get; set; }

    public string? Lane { get; set; }

    public string? Quarter { get; set; }

    public string? Hamlet { get; set; }

    public string? Commute { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? Town { get; set; }

    public string? Suburb { get; set; }

    public string? Postcode { get; set; }

    public string? City { get; set; }

    public string DivisionId { get; set; } = null!;

    public string CountryId { get; set; } = null!;

    public byte Variant { get; set; }

    public virtual Locality? Country { get; set; }

    public virtual LocalityDivision? Division { get; set; }

    public virtual ICollection<ProfileAddress> ProfileAddresses { get; set; } = new List<ProfileAddress>();
}
