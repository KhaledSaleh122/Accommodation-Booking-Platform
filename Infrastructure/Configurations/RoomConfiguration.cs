using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.Property(room => room.Thumbnail)
                .IsRequired();           
            builder.Property(room => room.AdultCapacity)
                .IsRequired();            
            builder.Property(room => room.ChildrenCapacity)
                .IsRequired();
            builder.Property(Room => Room.RoomNumber)
                .HasMaxLength(10);         
            builder.Property(Room => Room.Status)
                .IsRequired();
            builder.HasKey(pro => new { pro.HotelId, pro.RoomNumber });
            builder.HasOne(o => o.Hotel)
                   .WithMany(o => o.Rooms)
                   .HasForeignKey(o => o.HotelId);
        }
    }
}
