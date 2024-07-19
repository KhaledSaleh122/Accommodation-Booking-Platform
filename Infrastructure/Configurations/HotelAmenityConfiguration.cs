using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class HotelAmenityConfiguration : IEntityTypeConfiguration<HotelAmenity>
    {
        public void Configure(EntityTypeBuilder<HotelAmenity> builder)
        {
            builder.HasKey(ha => new { ha.HotelId, ha.AmenityId });
            builder.ToTable("HotelAmenity");
        }
    }
}
