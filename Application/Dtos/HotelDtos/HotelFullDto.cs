using Application.Dtos.AmenityDtos;
using Application.Dtos.ReviewDtos;
using Application.Dtos.RoomDtos;

namespace Application.Dtos.HotelDtos
{
    public class HotelFullDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Images { get; set; }
        public string Thumbnail { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public string HotelType { get; set; }
        public decimal PricePerNight { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<AmenityDto> Amenities { get; set; }

        public ICollection<RoomWithBookingDto> Rooms { get; set; }

        public ICollection<ReviewWithUserIdDto> Reviews { get; set; }
    }
}
