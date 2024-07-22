using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.Property(review => review.Rating)
                .IsRequired(true);            
            builder.Property(review => review.UserId)
                .IsRequired(true);
            builder.Property(review => review.Comment)
                .IsRequired(false);

            builder.HasKey(review => new { review.HotelId , review.UserId });
        }
    }
}
