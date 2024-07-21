using Application.Dtos.HotelDtos;
using Domain.Enums;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotels
{
    public class GetHotelsQuery: IRequest<(IEnumerable<HotelDto>, int, int, int)>
    {
        public GetHotelsQuery(
                int page, 
                int pageSize, 
                decimal minPrice,
                decimal? maxPrice,
                string? city, 
                string? country,
                HotelType[] hotelType, 
                string? hotelName,
                string? owner,
                int[] aminites
            )
        {
            Page = page;
            PageSize = pageSize;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            City = city;
            Country = country;
            HotelType = hotelType;
            HotelName = hotelName;
            Owner = owner;
            Aminites = aminites;
        }
        public GetHotelsQuery() {
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
