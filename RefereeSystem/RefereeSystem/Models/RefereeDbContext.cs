using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RefereeSystem.Models;

public partial class RefereeDbContext : DbContext
{
    public RefereeDbContext()
    {
    }

    public RefereeDbContext(DbContextOptions<RefereeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Assignments_pkey");

            entity.Property(e => e.Function).HasMaxLength(50);

            entity.HasOne(d => d.Match).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.MatchId)
                .HasConstraintName("fk_match");

            entity.HasOne(d => d.Referee).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.RefereeId)
                .HasConstraintName("fk_referee");
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Matches_pkey");

            entity.Property(e => e.AwayTeam).HasMaxLength(100);
            entity.Property(e => e.HomeTeam).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MatchDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ZAPLANOWANY'::character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
