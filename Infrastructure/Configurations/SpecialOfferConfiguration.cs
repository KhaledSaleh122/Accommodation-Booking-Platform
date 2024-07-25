using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class SpecialOfferConfiguration : IEntityTypeConfiguration<SpecialOffer>
    {
        public void Configure(EntityTypeBuilder<SpecialOffer> builder)
        {
            builder.Property(sf => sf.OfferType)
                .IsRequired(true);
            builder.Property(sf => sf.DiscountPercentage)
                .IsRequired(true);
            builder.Property(sf => sf.ExpireDate)
                .IsRequired(true);
            builder.Property(sf => sf.HotelId)
                .IsRequired(true);
            builder.HasKey(sf => sf.Id);
        }
    }
}
