using System;
using System.Collections.Generic;

namespace Halogen.DbModels
{
    public partial class Account
    {
        public Account()
        {
            AccountRoleAccounts = new HashSet<AccountRole>();
            AccountRoleCreatedBies = new HashSet<AccountRole>();
            ChallengeResponses = new HashSet<ChallengeResponse>();
            Challenges = new HashSet<Challenge>();
            Preferences = new HashSet<Preference>();
            Profiles = new HashSet<Profile>();
            Roles = new HashSet<Role>();
            TrustedDevices = new HashSet<TrustedDevice>();
        }

        public string Id { get; set; } = null!;
        public string UniqueIdentifier { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? EmailAddressToken { get; set; }
        public DateTime? EmailAddressTokenTimestamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Username { get; set; } = null!;
        public string NormalizedUsername { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public string? OneTimePassword { get; set; }
        public DateTime? OneTimePasswordTimestamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFaSecretKey { get; set; }
        public string? RecoveryToken { get; set; }
        public DateTime? RecoveryTokenTimestamp { get; set; }
        public byte LoginFailedCount { get; set; }
        public byte LockOutCount { get; set; }
        public bool LockOutEnabled { get; set; }
        public DateTime? LockOutOn { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ICollection<AccountRole> AccountRoleAccounts { get; set; }
        public virtual ICollection<AccountRole> AccountRoleCreatedBies { get; set; }
        public virtual ICollection<ChallengeResponse> ChallengeResponses { get; set; }
        public virtual ICollection<Challenge> Challenges { get; set; }
        public virtual ICollection<Preference> Preferences { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<TrustedDevice> TrustedDevices { get; set; }
    }
}
