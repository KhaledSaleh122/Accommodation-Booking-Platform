using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Queries.TopVisitedCities
{
    public class GetTopVisitedCitiesQuery : IRequest<IEnumerable<CityTopDto>>
    {

    }
}
