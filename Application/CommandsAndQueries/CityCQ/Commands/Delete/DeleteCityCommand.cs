using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Commands.Delete
{
    public class DeleteCityCommand(int id) : IRequest<CityDto>
    {
        public int Id { get; set; } = id;
    }
}
