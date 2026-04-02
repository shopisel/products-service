using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Data.Migrations;

[DbContext(typeof(ProductServiceDbContext))]
[Migration("20260402152000_AddCategoriesAndProductFields")]
public partial class AddCategoriesAndProductFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "categories",
            columns: table => new
            {
                id = table.Column<string>(type: "varchar", nullable: false),
                name = table.Column<string>(type: "varchar", nullable: false),
                image = table.Column<string>(type: "varchar", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_categories", x => x.id);
            });

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image) VALUES ('cat_uncategorized', 'Uncategorized', '') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.AddColumn<string>(
            name: "barcode",
            table: "products",
            type: "varchar",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "category_id",
            table: "products",
            type: "varchar",
            nullable: false,
            defaultValue: "cat_uncategorized");

        migrationBuilder.DropColumn(
            name: "image",
            table: "products");

        migrationBuilder.CreateIndex(
            name: "IX_products_category_id",
            table: "products",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "IX_products_name",
            table: "products",
            column: "name");

        migrationBuilder.AddForeignKey(
            name: "FK_products_categories_category_id",
            table: "products",
            column: "category_id",
            principalTable: "categories",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_products_categories_category_id",
            table: "products");

        migrationBuilder.DropTable(
            name: "categories");

        migrationBuilder.DropIndex(
            name: "IX_products_category_id",
            table: "products");

        migrationBuilder.DropIndex(
            name: "IX_products_name",
            table: "products");

        migrationBuilder.DropColumn(
            name: "barcode",
            table: "products");

        migrationBuilder.DropColumn(
            name: "category_id",
            table: "products");

        migrationBuilder.AddColumn<string>(
            name: "image",
            table: "products",
            type: "varchar",
            nullable: false,
            defaultValue: string.Empty);
    }
}
