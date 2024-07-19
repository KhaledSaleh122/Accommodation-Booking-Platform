using Application.Dtos.HotelDtos;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotelById
{
    public class GetHotelByIdQuery : IRequest<HotelFullDto?>
    {
        public GetHotelByIdQuery(int hotelId)
        {
            HotelId = hotelId;
        }

        public GetHotelByIdQuery() { 
        
        }

        public int HotelId { get; }
    }
}
