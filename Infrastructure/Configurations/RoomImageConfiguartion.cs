using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class RoomImageConfiguartion : IEntityTypeConfiguration<RoomImage>
    {
        public void Configure(EntityTypeBuilder<RoomImage> builder)
        {
            builder.Property(rI => rI.Path)
                .IsRequired();
            builder.HasOne(o => o.room)
                .WithMany(o => o.Images)
                .HasForeignKey(f => new { f.HotelId, f.RoomNumber });
        }
    }
}
