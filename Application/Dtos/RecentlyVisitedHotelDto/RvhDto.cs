using Application.Dtos.HotelDtos;

namespace Application.Dtos.RecentlyVisitedHotelDto
{
    public class RvhDto
    {
        public DateTime VisitedDate { get; set; }
        public HotelMinWithRatingDto hotel { get; set; }
    }
}
