using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Data.Migrations;

[DbContext(typeof(ProductServiceDbContext))]
partial class ProductServiceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.5");

        modelBuilder.Entity("ProductService.Data.Entities.ProductEntity", b =>
        {
            b.Property<string>("Id")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("id");

            b.Property<string>("Image")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("image");

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("name");

            b.HasKey("Id");

            b.ToTable("products");
        });
    }
}

