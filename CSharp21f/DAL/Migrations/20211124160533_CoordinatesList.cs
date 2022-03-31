using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class CoordinatesList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "Ship");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "Ship");

            migrationBuilder.AddColumn<string>(
                name: "Coordinates",
                table: "Ship",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "Ship");

            migrationBuilder.AddColumn<int>(
                name: "PositionX",
                table: "Ship",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionY",
                table: "Ship",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
