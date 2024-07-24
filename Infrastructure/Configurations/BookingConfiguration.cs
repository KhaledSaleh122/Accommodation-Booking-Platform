using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.Property(booking => booking.StartDate)
                .IsRequired(true);
            builder.Property(booking => booking.EndDate)
                .IsRequired(true);
            builder.Property(booking => booking.RoomNumber)
                .IsRequired(true);
            builder.Property(booking => booking.HotelId)
                .IsRequired(true);
            builder.Property(booking => booking.UserId)
                .IsRequired(true);
            builder.HasOne(rel => rel.Room)
                .WithMany(rel => rel.Bookings)
                .HasForeignKey(rel => new { rel.HotelId, rel.RoomNumber });
        }
    }
}
