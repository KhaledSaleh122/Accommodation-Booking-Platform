using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class BookingRoomConfiguration : IEntityTypeConfiguration<BookingRoom>
    {
        public void Configure(EntityTypeBuilder<BookingRoom> builder)
        {
            builder.HasKey(k => new { k.BookingId, k.HotelId, k.RoomNumber });
            builder.HasOne(o => o.Booking)
                .WithMany(m => m.BookingRooms)
                .HasForeignKey(o => o.BookingId);
            builder.HasOne(o => o.Room)
                .WithMany(m => m.BookingRooms)
                .HasForeignKey(o => new { o.HotelId, o.RoomNumber });
        }
    }
}
