using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class HotelImageConfiguartion : IEntityTypeConfiguration<HotelImage>
    {
        public void Configure(EntityTypeBuilder<HotelImage> builder)
        {
            builder.Property(hI => hI.Path)
                .IsRequired();
        }
    }
}
