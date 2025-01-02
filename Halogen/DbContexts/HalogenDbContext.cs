using System;
using System.Collections.Generic;
using Halogen.DbModels;
using Microsoft.EntityFrameworkCore;

namespace Halogen.DbContexts;

public partial class HalogenDbContext : DbContext
{
    public HalogenDbContext()
    {
    }

    public HalogenDbContext(DbContextOptions<HalogenDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountRole> AccountRoles { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Challenge> Challenges { get; set; }

    public virtual DbSet<ChallengeResponse> ChallengeResponses { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Interest> Interests { get; set; }

    public virtual DbSet<Locality> Localities { get; set; }

    public virtual DbSet<LocalityDivision> LocalityDivisions { get; set; }

    public virtual DbSet<Preference> Preferences { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<ProfileAddress> ProfileAddresses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TrustedDevice> TrustedDevices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Account_Id");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Id, "UQ__Account__3214EC06190EBE71").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EmailAddressToken)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EmailAddressTokenTimestamp).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.HashPassword).HasMaxLength(100);
            entity.Property(e => e.LockOutOn).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.NormalizedUsername)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.OneTimePassword)
                .HasMaxLength(15)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.OneTimePasswordTimestamp).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PasswordSalt).HasMaxLength(30);
            entity.Property(e => e.RecoveryToken)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.RecoveryTokenTimestamp).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SecretCode)
                .HasMaxLength(10)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SecretCodeTimestamp).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.TwoFactorKeys)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.TwoFactorVerifyingTokens)
                .HasMaxLength(200)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UniqueCode).HasMaxLength(40);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AccountRole_Id");

            entity.ToTable("AccountRole");

            entity.HasIndex(e => e.Id, "UQ__AccountR__3214EC06F25067A0").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountId).HasMaxLength(65);
            entity.Property(e => e.CreatedById)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EffectiveUntil).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.RoleId).HasMaxLength(65);

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoleAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.AccountRoleCreatedBies).HasForeignKey(d => d.CreatedById);

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Address_Id");

            entity.ToTable("Address");

            entity.HasIndex(e => e.Id, "UQ__Address__3214EC065083E066").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.BuildingName)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Commute)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CountryId)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DivisionId)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Group)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Hamlet)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Lane)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PoBoxNumber)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Postcode)
                .HasMaxLength(20)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Quarter)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StreetAddress)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Suburb)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Town)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Country).WithMany(p => p.Addresses).HasForeignKey(d => d.CountryId);

            entity.HasOne(d => d.Division).WithMany(p => p.Addresses).HasForeignKey(d => d.DivisionId);
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChallengeQuestion_Id");

            entity.ToTable("Challenge");

            entity.HasIndex(e => e.Id, "UQ__Challeng__3214EC063650CCC3").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedById)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Question).HasMaxLength(200);

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.Challenges).HasForeignKey(d => d.CreatedById);
        });

        modelBuilder.Entity<ChallengeResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChallengeResponse_Id");

            entity.ToTable("ChallengeResponse");

            entity.HasIndex(e => e.Id, "UQ__Challeng__3214EC067F88ED28").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountId).HasMaxLength(65);
            entity.Property(e => e.ChallengeId).HasMaxLength(65);
            entity.Property(e => e.Response).HasMaxLength(50);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Account).WithMany(p => p.ChallengeResponses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Challenge).WithMany(p => p.ChallengeResponses)
                .HasForeignKey(d => d.ChallengeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Currency_Id");

            entity.ToTable("Currency");

            entity.HasIndex(e => e.Id, "UQ__Currency__3214EC06F9BBA3CA").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code).HasMaxLength(5);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Symbol).HasMaxLength(5);
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Interest_Id");

            entity.ToTable("Interest");

            entity.HasIndex(e => e.Id, "UQ__Interest__3214EC06E2A832A9").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(60);
            entity.Property(e => e.Description).HasMaxLength(250).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ParentId)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Interest_ParentId_Id");
        });

        modelBuilder.Entity<Locality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Locality_Id");

            entity.ToTable("Locality");

            entity.HasIndex(e => e.Id, "UQ__Locality__3214EC06284188CE").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsoCode2Char).HasMaxLength(5);
            entity.Property(e => e.IsoCode3Char).HasMaxLength(5);
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.PrimaryCurrencyId).HasMaxLength(65);
            entity.Property(e => e.SecondaryCurrencyId)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.TelephoneCode).HasMaxLength(5);

            entity.HasOne(d => d.PrimaryCurrency).WithMany(p => p.LocalityPrimaryCurrencies)
                .HasForeignKey(d => d.PrimaryCurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SecondaryCurrency).WithMany(p => p.LocalitySecondaryCurrencies).HasForeignKey(d => d.SecondaryCurrencyId);
        });

        modelBuilder.Entity<LocalityDivision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_State_Id");

            entity.ToTable("LocalityDivision");

            entity.HasIndex(e => e.Id, "UQ__Locality__3214EC0645C42DCC").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(10)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.LocalityId).HasMaxLength(65);
            entity.Property(e => e.Name).HasMaxLength(80);

            entity.HasOne(d => d.Locality).WithMany(p => p.LocalityDivisions)
                .HasForeignKey(d => d.LocalityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_State_Locality_LocalityId");
        });

        modelBuilder.Entity<Preference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Preference_Id");

            entity.ToTable("Preference");

            entity.HasIndex(e => e.Id, "UQ__Preferen__3214EC06472AD3B7").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountId).HasMaxLength(65);
            entity.Property(e => e.DataFormat).HasMaxLength(2000);
            entity.Property(e => e.Privacy).HasMaxLength(4000);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Account).WithMany(p => p.Preferences)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Profile_Id");

            entity.ToTable("Profile");

            entity.HasIndex(e => e.Id, "UQ__Profile__3214EC06C8EAE412").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountId).HasMaxLength(65);
            entity.Property(e => e.AvatarName)
                .HasMaxLength(150)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Company)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CoverName)
                .HasMaxLength(150)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DateOfBirth).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.GivenName)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Interests)
                .HasMaxLength(1500)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.NickName)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PhoneNumberToken)
                .HasMaxLength(10)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PhoneNumberTokenTimestamp).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Websites)
                .HasMaxLength(2000)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Account).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ProfileAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProfileAddress_Id");

            entity.ToTable("ProfileAddress");

            entity.HasIndex(e => e.Id, "UQ__ProfileA__3214EC062A0284F8").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddressId).HasMaxLength(65);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ProfileId).HasMaxLength(65);

            entity.HasOne(d => d.Address).WithMany(p => p.ProfileAddresses)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Profile).WithMany(p => p.ProfileAddresses)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Role_Id");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Id, "UQ__Role__3214EC06EDF83C94").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedById)
                .HasMaxLength(65)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.Roles).HasForeignKey(d => d.CreatedById);
        });

        modelBuilder.Entity<TrustedDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TrustedDevice_Id");

            entity.ToTable("TrustedDevice");

            entity.HasIndex(e => e.Id, "UQ__TrustedD__3214EC063E89CF9A").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountId).HasMaxLength(65);
            entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BrowserType)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DeviceLocation)
                .HasMaxLength(200)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DeviceType).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.OperatingSystem)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UniqueIdentifier)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Account).WithMany(p => p.TrustedDevices)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
