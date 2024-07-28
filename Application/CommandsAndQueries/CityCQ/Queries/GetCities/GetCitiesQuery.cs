using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCities
{
    public class GetCitiesQuery : IRequest<(IEnumerable<CityDto> Cities, int TotalRecords, int Page, int PageSize)>
    {

        public GetCitiesQuery(int page, int pageSize)
        {
            var pagination = new PaginationParameters(page, pageSize);
            Page = pagination.Page;
            PageSize = pagination.pageSize;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
