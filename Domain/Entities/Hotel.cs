#nullable disable

using Domain.Enums;

namespace Domain.Entities
{
    public sealed class Hotel : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public decimal PricePerNight { get; set; }
        public HotelType HotelType { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public ICollection<HotelImage> Images { get; set; }
        public ICollection<HotelAmenity> HotelAmenity { get; set; }

        public ICollection<Room> Rooms { get; set; }

        public ICollection<Review> Reviews { get; set; }
        public ICollection<RecentlyVisitedHotel> RecentlyVisitedHotels { get; set; }

        public ICollection<SpecialOffer> SpecialOffers { get; set; }

    }
}
