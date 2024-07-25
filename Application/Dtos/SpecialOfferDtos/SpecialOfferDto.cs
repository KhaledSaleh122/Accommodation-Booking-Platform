using Domain.Enums;

namespace Application.Dtos.SpecialOfferDtos
{
    public class SpecialOfferDto
    {
        public string Id { get; set; }
        public int DiscountPercentage { get; set; }
        public OfferType OfferType { get; set; }
        public DateOnly ExpireDate { get; set; }
    }
}
