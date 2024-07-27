using Application.Dtos.BookingDtos;
using Domain.Enums;
using MediatR;

namespace Application.CommandsAndQueries.BookingCQ.Queries.GetUserBookings
{
    public class GetUserBookingsQuery : IRequest<(IEnumerable<BookingDto>, int, int, int)>
    {
        public string UserId { get; set; }
        public DateOnly? StartDate{ get; set; }
        public DateOnly? EndDate { get; set; }

        public GetUserBookingsQuery(
        int page,
        int pageSize
    )
        {
            var pagination = new PaginationParameters(page, pageSize);
            Page = pagination.Page;
            PageSize = pagination.pageSize;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
