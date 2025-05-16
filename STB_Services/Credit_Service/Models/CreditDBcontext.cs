using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Credit_Service.Models;

public partial class CreditDBcontext : DbContext
{
    public CreditDBcontext()
    {
    }

    public CreditDBcontext(DbContextOptions<CreditDBcontext> options)
        : base(options)
    {
    }

    public virtual DbSet<Credit> Credits { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Repayment> Repayments { get; set; }

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
            entity.Property(e => e.CreditAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("credit_amount");
            entity.Property(e => e.DurationMonths).HasColumnName("duration_months");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.InterestRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("interest_rate");
            entity.Property(e => e.RemainingBalance)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("remaining_balance");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F93E83EF5");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NotificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("notification_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Repayment>(entity =>
        {
            entity.HasKey(e => e.RepaymentId).HasName("PK__Repaymen__C038F8D667B8A2F9");

            entity.Property(e => e.RepaymentId).HasColumnName("repayment_id");
            entity.Property(e => e.AmountRepaid)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("amount_repaid");
            entity.Property(e => e.CreditId).HasColumnName("credit_id");
            entity.Property(e => e.RepaymentDate).HasColumnName("repayment_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Credit).WithMany(p => p.Repayments)
                .HasForeignKey(d => d.CreditId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Repayment__credi__7E37BEF6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
