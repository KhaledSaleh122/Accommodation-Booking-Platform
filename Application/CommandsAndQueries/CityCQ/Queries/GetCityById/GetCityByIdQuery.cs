using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCityById
{
    public class GetCityByIdQuery(uint cityId) : IRequest<CityDto>
    {
        public uint CityId { get; } = cityId;
    }
}
