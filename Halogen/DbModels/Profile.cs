using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Profile
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? PhoneNumberToken { get; set; }

    public DateTime? PhoneNumberTokenTimestamp { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public string? GivenName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }

    public string? NickName { get; set; }

    public string? AvatarName { get; set; }

    public string? CoverName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public byte Gender { get; set; }

    public byte Ethnicity { get; set; }

    public string? Company { get; set; }

    public string? JobTitle { get; set; }

    public string? OccupationId { get; set; }

    public string? Websites { get; set; }

    public string? Interests { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Occupation? Occupation { get; set; }

    public virtual ICollection<ProfileAddress> ProfileAddresses { get; set; } = new List<ProfileAddress>();
}
