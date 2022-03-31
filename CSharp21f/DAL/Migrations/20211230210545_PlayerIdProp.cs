using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class PlayerIdProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "ShipQuantities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShipSizeX",
                table: "ShipQuantities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShipSizeY",
                table: "ShipQuantities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "ShipQuantities");

            migrationBuilder.DropColumn(
                name: "ShipSizeX",
                table: "ShipQuantities");

            migrationBuilder.DropColumn(
                name: "ShipSizeY",
                table: "ShipQuantities");
        }
    }
}
