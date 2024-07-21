using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateRoomImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomImage_Room_HotelId_RoomNumber",
                        columns: x => new { x.HotelId, x.RoomNumber },
                        principalTable: "Rooms",
                        principalColumns: new[] { "HotelId", "RoomNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomImage_HotelId_RoomNumber",
                table: "RoomImage",
                columns: new[] { "HotelId", "RoomNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomImage");
        }
    }
}
