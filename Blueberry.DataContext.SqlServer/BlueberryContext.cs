using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Blueberry.EntityModels;

namespace Blueberry.DataContext.SqlServer;

public partial class BlueberryContext : DbContext
{
    public BlueberryContext()
    {
    }

    public BlueberryContext(DbContextOptions<BlueberryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<MerchantProduct> MerchantProducts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleItem> SaleItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=Blueberry;Integrated Security=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasMany(d => d.Categories).WithMany(p => p.Merchants)
                .UsingEntity<Dictionary<string, object>>(
                    "MerchantCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MerchantCategory_Category"),
                    l => l.HasOne<Merchant>().WithMany()
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MerchantCategory_Merchant"),
                    j =>
                    {
                        j.HasKey("MerchantId", "CategoryId");
                        j.ToTable("MerchantCategory");
                    });
        });

        modelBuilder.Entity<MerchantProduct>(entity =>
        {
            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantProduct_Merchant");

            entity.HasOne(d => d.Product).WithMany(p => p.MerchantProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantProduct_Product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasOne(d => d.Merchant).WithMany(p => p.Sales)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Merchant");
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(d => d.Product).WithMany(p => p.SaleItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleItem_Product");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleItem_Sale");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
