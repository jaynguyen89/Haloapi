using System;
using System.Collections.Generic;
using Halogen;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MediaLibrary.DbContexts;

public partial class MediaLibraryDbContext : DbContext
{
    public MediaLibraryDbContext()
    {
    }

    public MediaLibraryDbContext(DbContextOptions<MediaLibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apitoken> Apitokens { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Coverphoto> Coverphotos { get; set; }

    public virtual DbSet<Userphoto> Userphotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Apitoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("apitoken");

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("uuid()")
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.AccountId)
                .HasMaxLength(65)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.TargetEndpoint)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("unix_timestamp()")
                .HasColumnType("int(11) unsigned");
            entity.Property(e => e.TokenString)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.ValidityDuration)
                .HasDefaultValueSql("'10'")
                .HasColumnType("tinyint(3) unsigned");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("attachment");

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("uuid()")
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.FileName)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.Location)
                .HasMaxLength(250)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
        });

        modelBuilder.Entity<Coverphoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("coverphoto");

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("uuid()")
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.FileName)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.Location)
                .HasMaxLength(250)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
        });

        modelBuilder.Entity<Userphoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("userphoto");

            entity.Property(e => e.Id)
                .HasMaxLength(65)
                .HasDefaultValueSql("uuid()")
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.FileName)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.Location)
                .HasMaxLength(250)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
