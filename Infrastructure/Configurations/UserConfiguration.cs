using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(user => user.UserName).HasMaxLength(50);
            builder.HasIndex(user => user.UserName).IsUnique();
            builder.Property(user => user.Email).HasMaxLength(50);
            builder.HasIndex(user => user.Email).IsUnique();
        }
    }
}
