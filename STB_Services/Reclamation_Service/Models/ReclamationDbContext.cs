using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Reclamation_Service.Models;

public partial class ReclamationDbContext : DbContext
{
    public ReclamationDbContext()
    {
    }

    public ReclamationDbContext(DbContextOptions<ReclamationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Reclamation> Reclamations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-0S5FJJA\\\\SQLEXPRESS,1433;Database=STB_BANK_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reclamation>(entity =>
        {
            entity.HasKey(e => e.ReclamationId).HasName("PK__Reclamat__88EE2B2CA018047E");

            entity.Property(e => e.ReclamationId).HasColumnName("reclamation_id");
            entity.Property(e => e.AccountId).HasColumnName("Account_id");
            entity.Property(e => e.AssignmentDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ReclamationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("reclamation_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
