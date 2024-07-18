using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCities
{
    public class GetCitiesQuery : IRequest<(IEnumerable<CityDto>, uint, uint, uint)>
    {
        public uint Page { get; set; }
        public uint PageSize { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
