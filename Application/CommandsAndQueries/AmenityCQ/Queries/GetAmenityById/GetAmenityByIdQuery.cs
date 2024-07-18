using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById
{
    public class GetAmenityByIdQuery(int amenityId) : IRequest<AmenityDto?>
    {
        public int AmenityId { get; set; } = amenityId;
    }
}
