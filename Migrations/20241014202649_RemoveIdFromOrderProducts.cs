using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesOrders.Migrations
{
    public partial class RemoveIdFromOrderProducts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderProducts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "OrderProducts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1"); // Ensure it's an identity column if needed
        }
    }
}
