using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Account
{
    public string Id { get; set; } = null!;

    public string UniqueCode { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public string? EmailAddressToken { get; set; }

    public DateTime? EmailAddressTokenTimestamp { get; set; }

    public string? SecretCode { get; set; }

    public DateTime? SecretCodeTimestamp { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? Username { get; set; }

    public string? NormalizedUsername { get; set; }

    public string PasswordSalt { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public string? OneTimePassword { get; set; }

    public DateTime? OneTimePasswordTimestamp { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string? TwoFactorKeys { get; set; }

    public string? TwoFactorVerifyingTokens { get; set; }

    public string? RecoveryToken { get; set; }

    public DateTime? RecoveryTokenTimestamp { get; set; }

    public byte LoginFailedCount { get; set; }

    public byte LockOutCount { get; set; }

    public bool LockOutEnabled { get; set; }

    public DateTime? LockOutOn { get; set; }

    public bool IsSuspended { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<AccountRole> AccountRoleAccounts { get; set; } = new List<AccountRole>();

    public virtual ICollection<AccountRole> AccountRoleCreatedBies { get; set; } = new List<AccountRole>();

    public virtual ICollection<ChallengeResponse> ChallengeResponses { get; set; } = new List<ChallengeResponse>();

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual ICollection<Preference> Preferences { get; set; } = new List<Preference>();

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
}
