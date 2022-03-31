using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameConfigs",
                columns: table => new
                {
                    GameConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BoardSizeX = table.Column<int>(type: "int", nullable: false),
                    BoardSizeY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.GameConfigId);
                });

            migrationBuilder.CreateTable(
                name: "GameBoards",
                columns: table => new
                {
                    GameBoardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GameConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameBoards", x => x.GameBoardId);
                    table.ForeignKey(
                        name: "FK_GameBoards_GameConfigs_GameConfigId",
                        column: x => x.GameConfigId,
                        principalTable: "GameConfigs",
                        principalColumn: "GameConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipConfigs",
                columns: table => new
                {
                    ShipConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ShipSizeX = table.Column<int>(type: "int", nullable: false),
                    ShipSizeY = table.Column<int>(type: "int", nullable: false),
                    GameConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipConfigs", x => x.ShipConfigId);
                    table.ForeignKey(
                        name: "FK_ShipConfigs_GameConfigs_GameConfigId",
                        column: x => x.GameConfigId,
                        principalTable: "GameConfigs",
                        principalColumn: "GameConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ship",
                columns: table => new
                {
                    ShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    GameBoardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ship", x => x.ShipId);
                    table.ForeignKey(
                        name: "FK_Ship_GameBoards_GameBoardId",
                        column: x => x.GameBoardId,
                        principalTable: "GameBoards",
                        principalColumn: "GameBoardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameBoards_GameConfigId",
                table: "GameBoards",
                column: "GameConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Ship_GameBoardId",
                table: "Ship",
                column: "GameBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipConfigs_GameConfigId",
                table: "ShipConfigs",
                column: "GameConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ship");

            migrationBuilder.DropTable(
                name: "ShipConfigs");

            migrationBuilder.DropTable(
                name: "GameBoards");

            migrationBuilder.DropTable(
                name: "GameConfigs");
        }
    }
}
