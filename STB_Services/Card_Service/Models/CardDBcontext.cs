using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Card_Service.Models;

public partial class CardDBcontext : DbContext
{
    public CardDBcontext()
    {
    }

    public CardDBcontext(DbContextOptions<CardDBcontext> options)
        : base(options)
    {
    }

    public virtual DbSet<Card> Cards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-0S5FJJA\\\\SQLEXPRESS,1433;Database=STB_BANK_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__Cards__BDF201DD912503F5");

            entity.HasIndex(e => e.CardNumber, "UQ__Cards__1E6E0AF49DC3013A").IsUnique();

            entity.Property(e => e.CardId).HasColumnName("card_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CardNumber)
                .HasMaxLength(16)
                .HasColumnName("card_number");
            entity.Property(e => e.CardType)
                .HasMaxLength(50)
                .HasColumnName("card_type");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
