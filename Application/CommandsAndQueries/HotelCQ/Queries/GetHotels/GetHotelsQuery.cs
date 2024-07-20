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
                int minPrice,
                int? maxPrice,
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
        public int MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? City { get; set; }
        public string? Country { get; }
        public HotelType[] HotelType { get; }
        public string? HotelName { get; }
        public string? Owner { get; }
        public int[] Aminites { get; }
    }
}
