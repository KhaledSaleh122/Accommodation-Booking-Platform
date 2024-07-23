using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecentlyVisitedHotelTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE TRIGGER trg_LimitRecentlyVisitedHotels
            ON RecentlyVisitedHotels
            INSTEAD OF INSERT
            AS
            BEGIN
                DECLARE @UserId NVARCHAR(450), @HotelId INT, @VisitedDate DATETIME;

                SELECT @UserId = inserted.UserId, @HotelId = inserted.HotelId, @VisitedDate = inserted.VisitedDate
                FROM inserted;

                IF EXISTS (SELECT 1 FROM RecentlyVisitedHotels WHERE UserId = @UserId AND HotelId = @HotelId)
                BEGIN
                    UPDATE RecentlyVisitedHotels
                    SET VisitedDate = @VisitedDate
                    WHERE UserId = @UserId AND HotelId = @HotelId;
                END
                ELSE
                BEGIN
                    IF (SELECT COUNT(*) FROM RecentlyVisitedHotels WHERE UserId = @UserId) >= 5
                    BEGIN
                        DELETE FROM RecentlyVisitedHotels
                        WHERE UserId = @UserId AND VisitedDate = (SELECT MIN(VisitedDate) FROM RecentlyVisitedHotels WHERE UserId = @UserId);
                    END

                    INSERT INTO RecentlyVisitedHotels (UserId, HotelId, VisitedDate)
                    VALUES (@UserId, @HotelId, @VisitedDate);
                END
            END;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER trg_LimitRecentlyVisitedHotels;");
        }
    }
}
