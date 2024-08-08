using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities
{
    public class GetAmenitiesQuery : IRequest<(IEnumerable<AmenityDto> Amenities, int TotalRecords, int Page, int PageSize)>
    {
        public GetAmenitiesQuery(int page, int pageSize)
        {
            var pagination = new PaginationParameters(page, pageSize);
            Page = pagination.Page;
            PageSize = pagination.pageSize;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
