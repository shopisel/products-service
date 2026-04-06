using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Data.Migrations;

[DbContext(typeof(ProductServiceDbContext))]
[Migration("20260406190000_AddCategoryHierarchy")]
public partial class AddCategoryHierarchy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "parent_category_id",
            table: "categories",
            type: "varchar",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_categories_parent_category_id",
            table: "categories",
            column: "parent_category_id");

        migrationBuilder.AddForeignKey(
            name: "FK_categories_categories_parent_category_id",
            table: "categories",
            column: "parent_category_id",
            principalTable: "categories",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-frescos', 'Frescos', '', NULL) ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-peixaria', 'Peixaria', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-talho', 'Talho', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-frutas', 'Frutas', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-legumes', 'Legumes', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-charcutaria', 'Charcutaria', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");

        migrationBuilder.Sql(
            "INSERT INTO categories (id, name, image, parent_category_id) VALUES ('cat-padaria-pastelaria', 'Padaria e Pastelaria', '', 'cat-frescos') ON CONFLICT (id) DO NOTHING;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_categories_categories_parent_category_id",
            table: "categories");

        migrationBuilder.DropIndex(
            name: "IX_categories_parent_category_id",
            table: "categories");

        migrationBuilder.DropColumn(
            name: "parent_category_id",
            table: "categories");
    }
}
