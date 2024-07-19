using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.Property(hotel => hotel.HotelType)
                .IsRequired(true);
            builder.Property<decimal>(hotel => hotel.PricePerNight)
                .IsRequired(true);
            builder.Property(hotel => hotel.Thumbnail)
                .IsRequired(true);
            builder.Property(hotel => hotel.Name)
                .IsRequired(true)
                .HasMaxLength(50);
            builder.Property(hotel => hotel.Owner)
                .IsRequired(true)
                .HasMaxLength(50);
            builder.Property(hotel => hotel.Address)
                .IsRequired(true)
                .HasMaxLength(100);
            builder.Property(hotel => hotel.Description)
                .IsRequired(true)
                .HasMaxLength(160);
        }
    }
}
