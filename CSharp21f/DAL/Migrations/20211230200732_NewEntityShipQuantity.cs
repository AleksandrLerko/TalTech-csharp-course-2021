using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class NewEntityShipQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShipQuantities",
                columns: table => new
                {
                    ShipQuantityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ShipConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipQuantities", x => x.ShipQuantityId);
                    table.ForeignKey(
                        name: "FK_ShipQuantities_ShipConfigs_ShipConfigId",
                        column: x => x.ShipConfigId,
                        principalTable: "ShipConfigs",
                        principalColumn: "ShipConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipQuantities_ShipConfigId",
                table: "ShipQuantities",
                column: "ShipConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipQuantities");
        }
    }
}
