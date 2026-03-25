using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

public class ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Id)
                .HasColumnName("id")
                .HasColumnType("varchar");

            entity.Property(product => product.Name)
                .HasColumnName("name")
                .HasColumnType("varchar");

            entity.Property(product => product.Image)
                .HasColumnName("image")
                .HasColumnType("varchar");
        });
    }
}
