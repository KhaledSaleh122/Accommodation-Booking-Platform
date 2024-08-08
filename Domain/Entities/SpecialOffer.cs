using Domain.Enums;

namespace Domain.Entities
{
    public class SpecialOffer : BaseEntity
    {
        public string Id { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int DiscountPercentage { get; set; }
        public OfferType  OfferType { get; set; }

        public DateOnly ExpireDate { get; set; }
        public ICollection<Booking> bookings { get; set; }
    }
}
