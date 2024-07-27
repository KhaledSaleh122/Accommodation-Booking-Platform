using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignBookingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Rooms_HotelId_RoomNumber",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelId_RoomNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "OriginalPrice",
                table: "Bookings",
                newName: "OriginalTotalPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedTotalPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SpecialOfferId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingRooms",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRooms", x => new { x.BookingId, x.HotelId, x.RoomNumber });
                    table.ForeignKey(
                        name: "FK_BookingRooms_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingRooms_Rooms_HotelId_RoomNumber",
                        columns: x => new { x.HotelId, x.RoomNumber },
                        principalTable: "Rooms",
                        principalColumns: new[] { "HotelId", "RoomNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SpecialOfferId",
                table: "Bookings",
                column: "SpecialOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRooms_HotelId_RoomNumber",
                table: "BookingRooms",
                columns: new[] { "HotelId", "RoomNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_SpecialOffers_SpecialOfferId",
                table: "Bookings",
                column: "SpecialOfferId",
                principalTable: "SpecialOffers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_SpecialOffers_SpecialOfferId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "BookingRooms");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SpecialOfferId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountedTotalPrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SpecialOfferId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "OriginalTotalPrice",
                table: "Bookings",
                newName: "OriginalPrice");

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Bookings",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId_RoomNumber",
                table: "Bookings",
                columns: new[] { "HotelId", "RoomNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Rooms_HotelId_RoomNumber",
                table: "Bookings",
                columns: new[] { "HotelId", "RoomNumber" },
                principalTable: "Rooms",
                principalColumns: new[] { "HotelId", "RoomNumber" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
