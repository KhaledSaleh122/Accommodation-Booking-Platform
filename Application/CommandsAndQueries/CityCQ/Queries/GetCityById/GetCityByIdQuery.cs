using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCityById
{
    public class GetCityByIdQuery : IRequest<CityDto>
    {
        public int CityId { get; set; }
    }
}
