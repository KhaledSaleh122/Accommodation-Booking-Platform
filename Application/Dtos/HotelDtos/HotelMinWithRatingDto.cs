using Application.Dtos.AmenityDtos;

namespace Application.Dtos.HotelDtos
{
    public class HotelMinWithRatingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public string HotelType { get; set; }

        public decimal PricePerNight { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public double Rating { get; set; }
    }
}
