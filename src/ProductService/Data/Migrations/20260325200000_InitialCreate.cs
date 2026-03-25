using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Data.Migrations;

[DbContext(typeof(ProductServiceDbContext))]
[Migration("20260325200000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<string>(type: "varchar", nullable: false),
                name = table.Column<string>(type: "varchar", nullable: false),
                image = table.Column<string>(type: "varchar", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_products", x => x.id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "products");
    }
}

