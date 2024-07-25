using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
#nullable disable
    public sealed class User : IdentityUser
    {
        public string Thumbnail { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<RecentlyVisitedHotel> RecentlyVisitedHotels { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
