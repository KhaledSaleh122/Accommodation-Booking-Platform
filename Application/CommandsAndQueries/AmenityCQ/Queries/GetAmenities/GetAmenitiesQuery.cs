using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities
{
    public class GetAmenitiesQuery(uint page, uint pageSize) : IRequest<(IEnumerable<AmenityDto>, uint, uint, uint)>
    {
        public uint Page { get; set; } = page;
        public uint PageSize { get; set; } = pageSize;
    }
}
