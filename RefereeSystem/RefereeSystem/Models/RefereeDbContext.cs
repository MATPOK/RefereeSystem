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
    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- KONFIGURACJA ASSIGNMENTS ---
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

        // --- KONFIGURACJA MATCHES (TUTAJ ZMIANY!) ---
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Matches_pkey");

            // Usunąłem konfigurację MaxLength dla HomeTeam/AwayTeam, bo to już nie są stringi!

            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MatchDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ZAPLANOWANY'::character varying");

            // NOWOŚĆ: Konfiguracja relacji do tabeli Teams

            // Relacja dla Gospodarza
            entity.HasOne(d => d.HomeTeam)
                .WithMany() // Zespół może grać w wielu meczach
                .HasForeignKey(d => d.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict) // Ważne: Zapobiega usuwaniu kaskadowemu
                .HasConstraintName("FK_Matches_Teams_Home");

            // Relacja dla Gościa
            entity.HasOne(d => d.AwayTeam)
                .WithMany()
                .HasForeignKey(d => d.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict) // Ważne: Zapobiega usuwaniu kaskadowemu
                .HasConstraintName("FK_Matches_Teams_Away");
        });

        // --- KONFIGURACJA USERS ---
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

        // --- KONFIGURACJA TEAMS (Opcjonalnie, dla porządku) ---
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Teams_pkey");
            entity.Property(e => e.Name).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}