using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Transaction_Service.Model;

public partial class TransactionDBContext : DbContext
{
    public TransactionDBContext()
    {
    }

    public TransactionDBContext(DbContextOptions<TransactionDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionTemp> TransactionTemps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-0S5FJJA\\SQLEXPRESS,1433;Database=STB_BANK_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AF5AB54670");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Motif)
                .HasMaxLength(255)
                .HasColumnName("motif");
            entity.Property(e => e.TargetAccountId).HasColumnName("targetAccountId");
            entity.Property(e => e.TargetEmail).HasMaxLength(255);
            entity.Property(e => e.TargetRib).HasColumnName("Target_Rib");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
        });

        modelBuilder.Entity<TransactionTemp>(entity =>
        {
            entity.HasKey(e => e.TempTransactionId).HasName("PK__Transact__D405B7C0942BACB9");

            entity.ToTable("TransactionTemp");

            entity.Property(e => e.TempTransactionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InitiationDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Motif).HasMaxLength(255);
            entity.Property(e => e.TargetEmail).HasMaxLength(255);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
