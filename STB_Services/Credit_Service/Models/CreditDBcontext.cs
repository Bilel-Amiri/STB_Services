using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Credit_Service.Models;

public partial class CreditDBContext : DbContext
{
    public CreditDBContext()
    {
    }

    public CreditDBContext(DbContextOptions<CreditDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Credit> Credits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-0S5FJJA\\SQLEXPRESS,1433;Database=STB_BANK_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credit>(entity =>
        {
            entity.HasKey(e => e.CreditId).HasName("PK__Credit__C15A9C3691511FCE");

            entity.ToTable("Credit");

            entity.Property(e => e.CreditId).HasColumnName("credit_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AmortizationType)
                .HasMaxLength(20)
                .HasDefaultValue("constante");
            entity.Property(e => e.Cin)
                .HasMaxLength(20)
                .HasColumnName("cin");
            entity.Property(e => e.CreditAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("credit_amount");
            entity.Property(e => e.CreditType)
                .HasMaxLength(50)
                .HasDefaultValue("Crédit auto");
            entity.Property(e => e.DurationMonths).HasColumnName("duration_months");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(20)
                .HasColumnName("marital_status");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
