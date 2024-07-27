using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole,string>
    {
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Hotel> Hotels { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<HotelAmenity> HotelAmenity { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<SpecialOffer> SpecialOffers { get; set; }
        public DbSet<BookingRoom> BookingRooms { get; set; }

        public DbSet<RecentlyVisitedHotel> RecentlyVisitedHotels { get; set; }
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        }
    }
}
