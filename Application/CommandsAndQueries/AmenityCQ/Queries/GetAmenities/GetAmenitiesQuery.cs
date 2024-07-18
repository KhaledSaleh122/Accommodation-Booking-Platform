using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities
{
    public class GetAmenitiesQuery(int page, int pageSize) : IRequest<(IEnumerable<AmenityDto>, int, int, int)>
    {
        public int Page { get; set; } = page;
        public int PageSize { get; set; } = pageSize;
    }
}
