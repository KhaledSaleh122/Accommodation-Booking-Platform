using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class RecentlyVisitedHotelConfiguration : IEntityTypeConfiguration<RecentlyVisitedHotel>
    {
        public void Configure(EntityTypeBuilder<RecentlyVisitedHotel> builder)
        {
            builder.HasKey(rvh => new { rvh.HotelId, rvh.UserId });
            builder.Property(rvh => rvh.VisitedDate)
                .IsRequired(true);
        }
    }
}
