using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById
{
    public class GetAmenityByIdQuery(uint amenityId) : IRequest<AmenityDto?>
    {
        public uint AmenityId { get; set; } = amenityId;
    }
}
