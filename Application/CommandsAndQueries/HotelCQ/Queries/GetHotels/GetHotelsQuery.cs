using Application.Dtos.HotelDtos;
using Domain.Enums;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotels
{
    public class GetHotelsQuery: IRequest<(IEnumerable<HotelDto>, int, int, int)>
    {
        public GetHotelsQuery(
                int page, 
                int pageSize
            )
        {
            var pagination = new PaginationParameters(page, pageSize);
            Page = pagination.Page;
            PageSize = pagination.pageSize;
            Aminites = [];
            HotelType = [];
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public decimal MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public HotelType[] HotelType { get; set; }
        public string? HotelName { get; set; }
        public string? Owner { get; set; }
        public int[] Aminites { get; set; }
    }
}
