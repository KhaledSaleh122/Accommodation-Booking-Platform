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
            builder.Property(booking => booking.SpecialOfferId)
                .IsRequired(false);            
            builder.Property(booking => booking.OriginalTotalPrice)
                .IsRequired(true);

            builder.Property(booking => booking.UserId)
                .IsRequired(true);
        }
    }
}
