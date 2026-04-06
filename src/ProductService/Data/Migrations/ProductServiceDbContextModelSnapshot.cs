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

        modelBuilder.Entity("ProductService.Data.Entities.CategoryEntity", b =>
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

            b.Property<string>("ParentCategoryId")
                .HasColumnType("varchar")
                .HasColumnName("parent_category_id");

            b.HasKey("Id");

            b.HasIndex("ParentCategoryId")
                .HasDatabaseName("IX_categories_parent_category_id");

            b.ToTable("categories");
        });

        modelBuilder.Entity("ProductService.Data.Entities.ProductEntity", b =>
        {
            b.Property<string>("Id")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("id");

            b.Property<string>("Barcode")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("barcode");

            b.Property<string>("Image")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("image");

            b.Property<string>("CategoryId")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("category_id");

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("varchar")
                .HasColumnName("name");

            b.HasKey("Id");

            b.HasIndex("CategoryId")
                .HasDatabaseName("IX_products_category_id");

            b.HasIndex("Name")
                .HasDatabaseName("IX_products_name");

            b.ToTable("products");
        });

        modelBuilder.Entity("ProductService.Data.Entities.ProductEntity", b =>
        {
            b.HasOne("ProductService.Data.Entities.CategoryEntity", "Category")
                .WithMany("Products")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Category");
        });

        modelBuilder.Entity("ProductService.Data.Entities.CategoryEntity", b =>
        {
            b.HasOne("ProductService.Data.Entities.CategoryEntity", "ParentCategory")
                .WithMany("Subcategories")
                .HasForeignKey("ParentCategoryId")
                .OnDelete(DeleteBehavior.Restrict);

            b.Navigation("Products");

            b.Navigation("ParentCategory");

            b.Navigation("Subcategories");
        });
    }
}

