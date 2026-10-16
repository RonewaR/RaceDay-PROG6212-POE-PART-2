using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RaceDay_.API.Models;

namespace RaceDay_.API.Data;

public partial class RaceDayDbContext : DbContext
{
    public RaceDayDbContext()
    {
    }

    public RaceDayDbContext(
        DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        // Database configuration is provided by Program.cs.
        // Automated tests can supply their own database provider.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId)
                .HasName("PK__Categori__19093A0B94E12469");

            entity.Property(e => e.DistanceKm)
                .HasColumnType("decimal(5, 2)");

            entity.Property(e => e.EntryFee)
                .HasColumnType("decimal(8, 2)");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.StartTime)
                .HasColumnType("datetime");

            entity.HasOne(d => d.Event)
                .WithMany(p => p.Categories)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Categories_Events");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId)
                .HasName("PK__Enrolmen__5C0E5FCF0B8EC42A");

            entity.HasIndex(
                e => new { e.UserId, e.CategoryId },
                "UQ_User_Category")
                .IsUnique();

            entity.Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Confirmed");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Categories");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Users");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId)
                .HasName("PK__Events__7944C810958ACDEE");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.Property(e => e.EventDate)
                .HasColumnType("datetime");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Organiser)
                .WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganiserId)
                .HasConstraintName("FK_Events_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId)
                .HasName("PK__Payments__9B556A383E810BC6");

            entity.HasIndex(
                e => e.EnrolmentId,
                "UQ__Payments__5C0E5FCE8FDD4A02")
                .IsUnique();

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(8, 2)");

            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Paid");

            entity.HasOne(d => d.Enrolment)
                .WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.EnrolmentId)
                .HasConstraintName("FK_Payments_Enrolments");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId)
                .HasName("PK__Results__9769020886C2F6B2");

            entity.HasIndex(
                e => e.EnrolmentId,
                "UQ__Results__5C0E5FCE413015A1")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.IsDnf)
                .HasColumnName("IsDNF");

            entity.HasOne(d => d.Enrolment)
                .WithOne(p => p.Result)
                .HasForeignKey<Result>(d => d.EnrolmentId)
                .HasConstraintName("FK_Results_Enrolments");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId)
                .HasName("PK__Users__1788CC4C80BCCF1E");

            entity.HasIndex(
                e => e.Email,
                "UQ__Users__A9D1053484430EA4")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}