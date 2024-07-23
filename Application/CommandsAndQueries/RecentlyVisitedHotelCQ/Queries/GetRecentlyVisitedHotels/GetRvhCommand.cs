using Application.Dtos.RecentlyVisitedHotelDto;
using MediatR;

namespace Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels
{
    public class GetRvhCommand : IRequest<IEnumerable<RvhDto>>
    {
        public string UserId { get; set; }
    }
}
