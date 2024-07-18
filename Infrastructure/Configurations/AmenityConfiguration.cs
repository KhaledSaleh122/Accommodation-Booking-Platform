using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal sealed class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.Property(amenity => amenity.Name)
                .HasMaxLength(60)
                .IsRequired(true);
            builder.Property(amenity => amenity.Description)
                .HasMaxLength(160)
                .IsRequired(true);
        }

    }
}
