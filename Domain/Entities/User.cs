using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
#nullable disable
    public sealed class User : IdentityUser
    {
        public ICollection<Review> Reviews { get; set; }
        public ICollection<RecentlyVisitedHotel> recentlyVisitedHotels { get; set; }
    }
}
