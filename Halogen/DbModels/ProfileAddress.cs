using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class ProfileAddress
{
    public string Id { get; set; } = null!;

    public string ProfileId { get; set; } = null!;

    public string AddressId { get; set; } = null!;

    public bool IsForPostage { get; set; }

    public bool IsForDelivery { get; set; }

    public bool IsForReturn { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Profile Profile { get; set; } = null!;
}
