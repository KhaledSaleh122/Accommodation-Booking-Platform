using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.Property(city => city.Name)
                .IsRequired(true)
                .HasMaxLength(50);
            builder.Property(city => city.Country)
                .IsRequired(true)
                .HasMaxLength(50);
            builder.Property(city => city.PostOffice)
                .IsRequired(true)
                .HasMaxLength(20);
            builder.HasIndex(city => new { city.Name, city.Country })
                .IsUnique();
            builder.HasIndex(city => city.PostOffice)
                .IsUnique();
        }
    }
}
