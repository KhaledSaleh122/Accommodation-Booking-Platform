using Application.Dtos.HotelDtos;

namespace Application.Dtos.SpecialOfferDtos
{
    public class FeaturedDealsDto
    {
        public string Id { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public DateOnly ExpireDate { get; set; }
        public HotelBaseDto Hotel { get; set; }
    }
}
