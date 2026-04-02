using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

public class ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : DbContext(options)
{
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(category => category.Id);

            entity.Property(category => category.Id)
                .HasColumnName("id")
                .HasColumnType("varchar");

            entity.Property(category => category.Name)
                .HasColumnName("name")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(category => category.Image)
                .HasColumnName("image")
                .HasColumnType("varchar")
                .IsRequired();
        });

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Id)
                .HasColumnName("id")
                .HasColumnType("varchar");

            entity.Property(product => product.Name)
                .HasColumnName("name")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(product => product.Barcode)
                .HasColumnName("barcode")
                .HasColumnType("varchar")
                .IsRequired();

            entity.Property(product => product.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("varchar")
                .IsRequired();

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(product => product.CategoryId)
                .HasDatabaseName("IX_products_category_id");

            entity.HasIndex(product => product.Name)
                .HasDatabaseName("IX_products_name");
        });
    }
}
